import { resolve } from 'path';
import { App } from 'aws-cdk-lib/core';
import { config } from 'dotenv';
import { ApiStack } from '../stacks/api';
import { DbStack } from '../stacks';

const envFilePath = resolve(__dirname, '../../.env');

config({ path: envFilePath });

const {
    AWS_ACCOUNT_ID,
    AWS_REGION,
    DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME
} = process.env;

if (!AWS_ACCOUNT_ID || !AWS_REGION) throw new Error('Environment is missing AWS config');

if (!DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME)
    throw new Error(`Environment is missing ${DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME} variable`);

const app = new App();

const apiStackProps = {
    env: {
        account: AWS_ACCOUNT_ID,
        region: AWS_REGION
    },
    environment: {
        dockerRegistryImageUriSsmParamName: DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME
    }
};

new DbStack(app, 'UserDbStack');
new ApiStack(app, 'UserApiStack', apiStackProps);