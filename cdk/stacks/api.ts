import { App, Stack } from 'aws-cdk-lib/core';
import { Vpc } from 'aws-cdk-lib/aws-ec2';
import { ApplicationLoadBalancedFargateService } from 'aws-cdk-lib/aws-ecs-patterns';
import { HttpIntegration, RestApi } from 'aws-cdk-lib/aws-apigateway';
import { CfnOutput } from 'aws-cdk-lib/core';
import { Cluster, ContainerImage, FargateTaskDefinition, LogDrivers } from 'aws-cdk-lib/aws-ecs';
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

    // Add a container to the task definition
    const container = taskDefinition.addContainer('UserServiceContainer', {
      image: ContainerImage.fromRegistry(dockerRegistryImageUriSsmParamName),
      logging: LogDrivers.awsLogs({ streamPrefix: 'user-service' }), // Configure logging to CloudWatch Logs,
    });

    // Expose a port
    container.addPortMappings({
      containerPort: 80
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
    });

    // Create an API Gateway
    const api = new RestApi(this, 'UserApiGateway');

    api.root.addProxy

    // Define API root
    const apiResource = api.root.addResource('api');

    // Define user resources
    const userResource = apiResource.addResource('users');
    const userByIdResource = userResource.addResource('{userId}');

    // Define integration
    const httpIntegration = new HttpIntegration(
      `http://${service.loadBalancer.loadBalancerDnsName}:80`,
    );

    // Define GET /users endpoint
    userResource.addMethod('GET', httpIntegration);

    // Define GET /users/{userId} endpoint
    userByIdResource.addMethod('GET', httpIntegration);

    // Define POST /users endpoint
    userResource.addMethod('POST', httpIntegration);

    // Define PUT /users/{userId} endpoint
    userByIdResource.addMethod('PUT', httpIntegration);

    // Define DELETE /users/{userId} endpoint
    userByIdResource.addMethod('DELETE', httpIntegration);

    // Output the endpoint URL
    new CfnOutput(this, 'UserServiceEndpoint', {
      value: service.loadBalancer.loadBalancerDnsName
    });

    new CfnOutput(this, 'UserApiGatewayEndpoint', {
      value: api.url
    });
  }
}
