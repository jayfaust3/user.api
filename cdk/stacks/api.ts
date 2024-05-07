import { App, Stack } from 'aws-cdk-lib/core';
import { Vpc } from 'aws-cdk-lib/aws-ec2';
import { ContainerImage } from 'aws-cdk-lib/aws-ecs';
import { ApplicationLoadBalancedFargateService } from 'aws-cdk-lib/aws-ecs-patterns';
import { HttpIntegration, RestApi } from 'aws-cdk-lib/aws-apigateway';
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

    // Create a Fargate service
    const fargateService = new ApplicationLoadBalancedFargateService(this, 'UserService', {
      vpc,
      taskImageOptions: {
        image: ContainerImage.fromRegistry(dockerRegistryImageUriSsmParamName),
        environment: {

        },
      },
      desiredCount: 2,
    });

    // Create an API Gateway
    const api = new RestApi(this, 'UserApiGateway');

    // Define API root
    const apiResource = api.root.addResource('api');

    // Define user resources
    const userResource = apiResource.addResource('users');
    const userByIdResource = userResource.addResource('{userId}');

    // Define integration
    const httpIntegration = new HttpIntegration(
      `http://${fargateService.loadBalancer.loadBalancerDnsName}`,
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
  }
}
