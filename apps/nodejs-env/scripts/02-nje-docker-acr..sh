###################################################################
# Docker Build
# DATE: 16-03-2026
# RESULT: Worked Fine!
 
# Move to the app directory
cd apps/nodejs-env/app
echo "CURRENT DIRECTORY: $(pwd)"

# repository name must be lowercase
app="nodejsenv"
cnt="cnt-${app}"
img="img-${app}"
tag="v1.0.0"

## [Start Docker if not already started]: build, force emulation when running on Mac
docker image build --platform linux/x86_64 -t $img -f Dockerfile .

## Run
docker container list 

# This will run the app.js which will automatically write the logs to the console
docker container run  --name $cnt \
    --platform linux/x86_64 -it $img

## Cleanup
docker container rm -f $cnt

## logs: follow logs as they're output
docker container logs -f $cnt


###################################################################

# Azure Container Registry 

# Variables


## RUN: login.azcli for .env variables, login and subscription selection
## Env Vars: TENANT, SUBSCRIPTION, ORG, LOCATION, PROJECT, RG

## Prexisting Container Registry: SEE: aca/aca-acr-env.azcli for details on creation
## Create the variables, note ACR can't have dashes in its name, so we need to remove them
# $ACR is defined in shared .env file 
tag="v1.0.0"
acr_repo="${ACR}.azurecr.io/${app}:${tag}"
 
## Tag locally, then push to ACR as repo
docker tag $img $acr_repo
# NOTE: If you have issues with login, check if the ACR was provisioned correctly in the portal
# If provisioning fails, use a DIFFERENT name, as it seems to get stuck
az acr login -n $ACR -g $RG 
docker push $acr_repo

## List the created repositories and tags in ACR
az acr repository list -n $ACR  -o table
az acr repository show-tags -n $ACR --repository $app -o table

