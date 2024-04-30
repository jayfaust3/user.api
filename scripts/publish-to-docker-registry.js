const { resolve } = require('path');
const { exec } = require('child_process');
const { config } = require('dotenv');
const { SSM } = require('aws-sdk');

const envFilePath = resolve(__dirname, '../.env');

config({ path: envFilePath });

const {
  DOCKER_REPOSITORY,
  DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME,
  DOCKER_USERNAME,
  DOCKER_PASSWORD
} = process.env;

if (
  !DOCKER_REPOSITORY ||
  !DOCKER_USERNAME ||
  !DOCKER_PASSWORD
) throw new Error(`Environment is missing Docker credentials`);

if (!DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME)
  throw new Error(`Environment is missing Docker registry image URI SSM param name`);

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

  console.log(`writing '${dockerImageURI}' to SSM for the key '${DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME}'`);

  const ssm = new SSM();

  // Create the parameter
  const putParameterParams = {
    Name: DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME,
    Value: dockerImageURI,
    Type: 'String',
    Overwrite: true, // Set to true to update an existing parameter, false to create a new one
  };

  // Write the parameter to Parameter Store
  ssm.putParameter(putParameterParams, (err, data) => {
    if (err) {
      console.error('Error writing parameter:', err);
    } else {
      console.log('Parameter written successfully:', data);
    }
  });
});