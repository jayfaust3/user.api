const { resolve } = require('path');
const { exec } = require('child_process');
const { config } = require('dotenv');
const { SSM } = require('aws-sdk');

const envFilePath = resolve(__dirname, '../.env');

config({ path: envFilePath });

const { DOCKER_REPOSITORY, DOCKER_USERNAME, DOCKER_PASSWORD } = process.env;

if (
    !DOCKER_REPOSITORY ||
    !DOCKER_USERNAME ||
    !DOCKER_PASSWORD
) throw new Error(`Environment is missing Docker credentials`);

const pathToDockerfile = resolve(__dirname, '../user.api/Dockerfile');

const imageName = 'user-service';

const buildTag = Date.now();

const dockerImageURI = `${DOCKER_REPOSITORY}:${buildTag}`;

const command = `
docker build -t ${imageName} -f ${pathToDockerfile} .

docker tag ${imageName} ${dockerImageURI}

echo ${DOCKER_PASSWORD} | docker login --username ${DOCKER_USERNAME} --password-stdin ${DOCKER_REPOSITORY}

docker push ${dockerImageURI}

docker logout ${DOCKER_REPOSITORY}
`;

console.log('running command', { command });

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