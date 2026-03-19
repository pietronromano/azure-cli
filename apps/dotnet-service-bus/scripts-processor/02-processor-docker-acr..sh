###################################################################
# Docker Build

# Move to the app directory
cd apps/dotnet-service-bus/ServiceBus.Processor
 
# repository name must be lowercase
app="sbprocessor"
cnt="cnt-${app}"
img="img-${app}"
tag="v1.0.0"

## [Start Docker if not already started]: build, force emulation when running on Mac

## NOTE! Notice the two ".." at the end instead of just one "." 
## This tells Docker to use the parent directory (dotnet-service-bus) as the build context, 
##which gives access to both ServiceBus.Sender and ServiceBus.Utils directories.
docker image build --platform linux/x86_64 -t $img -f Dockerfile ..

## Run
docker container list 
docker container run --name $cnt \
    --platform linux/x86_64 -it \
    --env-file .env \
    $img

## Cleanup
docker container rm -f $cnt

## logs: follow logs as they're output
docker container logs -f $cnt



###################################################################

# Azure Container Registry 

# Variables
## GOTO: the ROOT of the repo to pick up environment variables from .env file:
cd ../../../
## RUN: login.azcli for .env variables, login and subscription selection
echo $ENV_VARS
## Create the variables, note ACR can't have dashes in its name, so we need to remove them
tag="v1.0.0"
acr_repo="${ACR}.azurecr.io/${app}:${tag}"


## Prexisting Container Registry: SEE: aca/aca-acr-env.azcli for details on creation

## Tag locally, then push to ACR as repo
docker tag $img $acr_repo
az acr login -n $ACR -g $RG
docker push $acr_repo

## List the created repositories and tags in ACR
az acr repository list -n $ACR  -o table
az acr repository show-tags -n $ACR --repository $app -o table

