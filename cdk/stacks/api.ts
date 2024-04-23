import * as cdk from 'aws-cdk-lib/core';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as ecsPatterns from 'aws-cdk-lib/aws-ecs-patterns';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';

export class ApiStack extends cdk.Stack {
  constructor(scope: cdk.App, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // Create a VPC
    const vpc = new ec2.Vpc(this, 'UserServiceVpc', {
      maxAzs: 2,
    });

    // Create a Fargate service
    const fargateService = new ecsPatterns.ApplicationLoadBalancedFargateService(this, 'UserService', {
      vpc,
      taskImageOptions: {
        image: ecs.ContainerImage.fromRegistry('your-docker-image'), // Replace 'your-docker-image' with the URI of your Docker image
      },
    });

    // Create an API Gateway
    const api = new apigateway.RestApi(this, 'UserApiGateway');

    // Define user resource
    const userResource = api.root.addResource('users');

    // Define GET /users endpoint
    userResource.addMethod('GET', new apigateway.HttpIntegration(
        `http://${fargateService.loadBalancer.loadBalancerDnsName}/users`,
    ));

    // Define GET /users/{userId} endpoint
    const userByIdResource = userResource.addResource('{userId}');
    userByIdResource.addMethod('GET', new apigateway.HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users/{userId}`));

    // Define POST /users endpoint
    userResource.addMethod('POST', new apigateway.HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users`));

    // Define PUT /users/{userId} endpoint
    userByIdResource.addMethod('PUT', new apigateway.HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users/{userId}`));

    // Define DELETE /users/{userId} endpoint
    userByIdResource.addMethod('DELETE', new apigateway.HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users/{userId}`));
  }
}

const app = new cdk.App();
new ApiStack(app, 'UserApiStack');