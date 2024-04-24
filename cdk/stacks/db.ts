import { App, Stack, StackProps } from 'aws-cdk-lib/core';

export class DbStack extends Stack {
  constructor(scope: App, id: string, props?: StackProps) {
    super(scope, id, props);
  }
}
