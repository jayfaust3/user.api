import { config } from 'dotenv';
import { exec } from 'child_process';
import { SSM } from 'aws-sdk';
import { v4 as uuid } from 'uuid';

type PutParams = SSM.PutParameterRequest;
type PutResult = SSM.PutParameterResult;

const { DOCKER_REPOSITORY, DOCKER_USERNAME, DOCKER_PASSWORD } = process.env;

if (
    !DOCKER_REPOSITORY ||
    !DOCKER_USERNAME ||
    !DOCKER_PASSWORD
) throw new Error(`Environment is missing Docker credentials`);

config();

const pathToDockerfile = './user.api/Dockerfile';

const imageName = 'user-service';

const buildTag = uuid();

const localImage = `${imageName}:${buildTag}`;

const dockerImageURI = `${DOCKER_REPOSITORY}:${buildTag}`;

const command = `
docker build -t ${localImage} -f ${pathToDockerfile}

docker tag ${localImage} ${dockerImageURI}

echo ${DOCKER_PASSWORD} | docker login --username ${DOCKER_USERNAME} --password-stdin ${DOCKER_REPOSITORY}

docker push ${dockerImageURI}

docker logout ${DOCKER_REPOSITORY}
`;

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