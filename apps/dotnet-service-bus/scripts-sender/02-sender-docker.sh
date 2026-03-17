###################################################################
# Docker Build

# Move to the app directory
cd apps/dotnet-service-bus/ServiceBus.Sender

# repository name must be lowercase
app="sbsender"
cnt="cnt-${app}"
img="img-${app}"
host_port="8080"
container_port="8080"
tag="v1.0.0"

## [Start Docker if not already started]: build, force emulation when running on Mac

## NOTE! Notice the two ".." at the end instead of just one "." 
## This tells Docker to use the parent directory (dotnet-service-bus) as the build context, 
##which gives access to both ServiceBus.Sender and ServiceBus.Utils directories.
docker image build --platform linux/x86_64 -t $img -f Dockerfile ..

## Run
docker container list 
docker container run  --name $cnt \
    --platform linux/x86_64 -it \
    --env-file .env \
    -p $host_port:$container_port $img

## Cleanup
docker container rm -f $cnt

## logs: follow logs as they're output
docker container logs -f $cnt


## test on the hostport
curl http://localhost:$host_port/send?message=my-message?num=10

curl http://localhost:$host_port/process

###################################################################

# Azure Container Registry 

# Variables
## GOTO: the ROOT of the repo to pick up environment variables from .env file:
cd ../../../
## RUN: login.azcli for .env variables, login and subscription selection
## Env Vars: TENANT, SUBSCRIPTION, ORG, LOCATION, PROJECT, RG

## Create the variables, note ACR can't have dashes in its name, so we need to remove them
acr="${ORG}acr${PROJECT//-/}"
tag="v1.0.0"
acr_repo="${acr}.azurecr.io/${app}:${tag}"


## Prexisting Container Registry: SEE: aca/aca-acr-env.azcli for details on creation

## Tag locally, then push to ACR as repo
docker tag $img $acr_repo
az acr login -n $acr -g $RG
docker push $acr_repo

## List the created repositories and tags in ACR
az acr repository list -n $acr  -o table
az acr repository show-tags -n $acr --repository $app -o table

