import { StackProps } from 'aws-cdk-lib/core';

export interface ApiStackProps extends StackProps {
    environment: {
      dockerRegistryImageUriSsmParamName: string
    }
}
