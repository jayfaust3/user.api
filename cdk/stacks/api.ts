import { App, Stack, StackProps } from 'aws-cdk-lib/core';
import { StringParameter } from 'aws-cdk-lib/aws-ssm';
import { Vpc } from 'aws-cdk-lib/aws-ec2';
import { ContainerImage } from 'aws-cdk-lib/aws-ecs';
import * as ecsPatterns from 'aws-cdk-lib/aws-ecs-patterns';
import { HttpIntegration, RestApi } from 'aws-cdk-lib/aws-apigateway';

interface ApiStackProps extends StackProps {
  environment: {
    dockerRegistryImageUriSsmParamName: string
  }
}

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
    const fargateService = new ecsPatterns.ApplicationLoadBalancedFargateService(this, 'UserService', {
      vpc,
      taskImageOptions: {
        image: ContainerImage.fromRegistry(dockerRegistryImageUriSsmParamName),
      },
    });

    // Create an API Gateway
    const api = new RestApi(this, 'UserApiGateway');

    // Define user resource
    const userResource = api.root.addResource('users');

    // Define GET /users endpoint
    userResource.addMethod('GET', new HttpIntegration(
        `http://${fargateService.loadBalancer.loadBalancerDnsName}/api/users`,
    ));

    // Define GET /users/{userId} endpoint
    const userByIdResource = userResource.addResource('{userId}');
    userByIdResource.addMethod('GET', new HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users/{userId}`));

    // Define POST /users endpoint
    userResource.addMethod('POST', new HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users`));

    // Define PUT /users/{userId} endpoint
    userByIdResource.addMethod('PUT', new HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users/{userId}`));

    // Define DELETE /users/{userId} endpoint
    userByIdResource.addMethod('DELETE', new HttpIntegration(`${fargateService.loadBalancer.loadBalancerDnsName}/users/{userId}`));
  }
}
