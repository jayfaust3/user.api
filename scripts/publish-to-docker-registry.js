const { resolve } = require('path');
const { exec } = require('child_process');
const AWS = require('aws-sdk');
const { config } = require('dotenv');

const envFilePath = resolve(__dirname, '../.env');

config({ path: envFilePath });

const {
  // // AWS_ENDPOINT,
  AWS_PROFILE,
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

const imageName = 'user-service';

const buildTag = Date.now();

const dockerImageURI = `${DOCKER_REPOSITORY}:${buildTag}`;

// const ssm = new AWS.SSM({ endpoint: AWS_ENDPOINT });

// const putParameterParams = {
//   Name: DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME,
//   Value: dockerImageURI,
//   Type: 'String',
//   Overwrite: true,
// };

// ssm.putParameter(putParameterParams, (err, data) => {
//   if (err) {
//     console.error('Error writing parameter:', err);
//   } else {
//     const pathToDockerfile = resolve(__dirname, '../user.api/Dockerfile');

//     const publishCommand = `
//     echo ${DOCKER_PASSWORD} | docker login --username ${DOCKER_USERNAME} --password-stdin &&

//     docker build -t ${imageName} -f ${pathToDockerfile} . &&

//     docker tag ${imageName} ${dockerImageURI} &&

//     docker push ${dockerImageURI} &&

//     docker logout &&

//     aws ssm put-parameter --profile ${AWS_PROFILE} --name ${DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME} --value ${dockerImageURI} --type String --overwrite
//     `;

//     exec(publishCommand, (error, stdout, stderr) => {
//       if (error) {
//         console.log(`error: ${error.message}`);
//         return;
//       }
    
//       if (stderr) {
//         console.log(`stderr: ${stderr}`);
//         return;
//       }
    
//       console.log(`stdout: ${stdout}`);
//     });
//   }
// });

const pathToDockerfile = resolve(__dirname, '../user.api/Dockerfile');

const publishCommand = `
echo ${DOCKER_PASSWORD} | docker login --username ${DOCKER_USERNAME} --password-stdin &&

docker build -t ${imageName} -f ${pathToDockerfile} . &&

docker tag ${imageName} ${dockerImageURI} &&

docker push ${dockerImageURI} &&

docker logout &&

aws ssm put-parameter --profile ${AWS_PROFILE} --name ${DOCKER_REGISTRY_IMAGE_URI_SSM_PARAM_NAME} --value ${dockerImageURI} --type String --overwrite
`;

exec(publishCommand, (error, stdout, stderr) => {
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