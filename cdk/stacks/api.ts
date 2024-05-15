import { App, CfnOutput, Stack } from 'aws-cdk-lib/core';
import { Vpc } from 'aws-cdk-lib/aws-ec2';
import { ApplicationLoadBalancedFargateService } from 'aws-cdk-lib/aws-ecs-patterns';
import { ConnectionType, Cors, HttpIntegration, RestApi } from 'aws-cdk-lib/aws-apigateway';
import { Cluster, ContainerImage, FargateTaskDefinition, LogDrivers } from 'aws-cdk-lib/aws-ecs';
import { ApplicationLoadBalancer } from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import { StringParameter } from 'aws-cdk-lib/aws-ssm';
import { ApiStackProps } from '../types';

export class ApiStack extends Stack {
  constructor(scope: App, id: string, props: ApiStackProps) {
    super(scope, id, props);

    const { environment } = props;

    const {
      dockerRegistryImageUriSsmParamName
    } = environment;

    // Create a VPC
    const vpc = new Vpc(this, 'UserServiceVpc', {
      maxAzs: 2,
    });

    const cluster = new Cluster(this, 'UserServiceCluster', {
      vpc: vpc,
      clusterName: 'UserServiceCluster',
      containerInsights: true
    });

    // Create a Fargate task definition
    const taskDefinition = new FargateTaskDefinition(this, 'UserServiceTaskDefinition', {
      cpu: 256,
      memoryLimitMiB: 512,
    });
    
    const registryUri = StringParameter.valueForStringParameter(this, dockerRegistryImageUriSsmParamName);

    new CfnOutput(this, 'UserServiceImageRegistryUri', {
      value: registryUri
    });

    const image = ContainerImage.fromRegistry(registryUri);

    // Add a container to the task definition
    taskDefinition.addContainer('UserServiceContainer', {
      image,
      logging: LogDrivers.awsLogs({ streamPrefix: 'user-service' }), // Configure logging to CloudWatch Logs,
      environment: {
        DISABLE_AUTH: 'true',
        SERVICE_CALL_RETRY_COUNT: '3',
        OPENSEARCH_NODE_URIS: 'http://opensearch-master-node:9200',
        OPENSEARCH_USER_INDEX_NAME: 'users',
      },
      portMappings: [
        {
          containerPort: 80
        }
      ]
    });

    // Create a Fargate service
    const service = new ApplicationLoadBalancedFargateService(this, 'UserService', {
      cluster,
      taskDefinition,
      circuitBreaker: {
        rollback: true,
      },
      cpu: 256,
      memoryLimitMiB: 512,
      desiredCount: 1,
      publicLoadBalancer: true
    });

    const loadBalancer: ApplicationLoadBalancer = service.loadBalancer;

    const backendServiceUrl = `http://${loadBalancer.loadBalancerDnsName}`;

    // Create an API Gateway
    const api = new RestApi(this, 'UserApiGateway', {
      defaultCorsPreflightOptions: {
        allowOrigins: Cors.ALL_ORIGINS,
        allowMethods: Cors.ALL_METHODS,
        allowHeaders: Cors.DEFAULT_HEADERS
      },
    });

    // Define API root
    const apiResource = api.root.addResource('api');

    // Define user resources
    const userResource = apiResource.addResource('users');
    const userByIdResource = userResource.addResource('{userId}');

    // Define GET /users endpoint
    userResource.addMethod(
      'GET',
      new HttpIntegration(
        `${backendServiceUrl}/api/users`,
        {
          httpMethod: 'GET',
          options: {
            connectionType: ConnectionType.INTERNET
          },
        }
      )
    );

    // Define GET /users/{userId} endpoint
    userByIdResource.addMethod(
      'GET',
      new HttpIntegration(
        `${backendServiceUrl}/api/users/{userId}`,
        {
          httpMethod: 'GET',
          options: {
            connectionType: ConnectionType.INTERNET
          },
        }
      )
    );

    // Define POST /users endpoint
    userResource.addMethod(
      'POST',
      new HttpIntegration(
        `${backendServiceUrl}/api/users`,
        {
          httpMethod: 'POST',
          options: {
            connectionType: ConnectionType.INTERNET
          },
        }
      )
    );

    // Define PUT /users/{userId} endpoint
    userByIdResource.addMethod(
      'PUT',
      new HttpIntegration(
        `${backendServiceUrl}/api/users/{userId}`,
        {
          httpMethod: 'PUT',
          options: {
            connectionType: ConnectionType.INTERNET
          },
        }
      )
    );

    // Define DELETE /users/{userId} endpoint
    userByIdResource.addMethod(
      'DELETE',
      new HttpIntegration(
        `${backendServiceUrl}/api/users/{userId}`,
        {
          httpMethod: 'DELETE',
          options: {
            connectionType: ConnectionType.INTERNET
          },
        }
      )
    );

    // Output the endpoint URL
    new CfnOutput(this, 'UserServiceEndpoint', {
      value: loadBalancer.loadBalancerDnsName
    });

    new CfnOutput(this, 'UserApiGatewayEndpoint', {
      value: api.url
    });
  }
}
