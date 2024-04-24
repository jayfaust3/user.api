import { App } from 'aws-cdk-lib/core';
import { ApiStack } from '../stacks/api';
import { DbStack } from '../stacks';

const app = new App();
new DbStack(app, 'UserDbStack');
new ApiStack(app, 'UserApiStack');