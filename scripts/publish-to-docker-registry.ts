import { config } from 'dotenv';
import { exec } from 'child_process';
import { SSM } from 'aws-sdk';
import { v4 as uuid } from 'uuid';

type PutParams = SSM.PutParameterRequest;
type PutResult = SSM.PutParameterResult;

const { DOCKER_USERNAME, DOCKER_PASSWORD } = process.env;

if (!DOCKER_USERNAME || !DOCKER_PASSWORD) throw new Error(`Environment is missing Docker credentials`);

config();

const buildId = uuid();

const command = ``;

exec(command, (error, stdout, stderr) => {
    if (error) {
      console.log(`error: ${error.message}`);
      return;
    }
    if (stderr) {
      console.log(`stderr: ${stderr}`);
      return;
    }
    console.log(`stdout: ${stdout}`);
});