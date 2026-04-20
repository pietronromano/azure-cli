###################################################################
# Docker Build

# Move to the app directory
cd apps/dotnet-service-bus/SB.ExampleProcessor
 
# repository name must be lowercase
app="sbproc"
cnt="cnt-${app}"
img="img-${app}"
tag="v3.0.0-sessions"

## [Start Docker if not already started]: build, force emulation when running on Mac

## NOTE! Notice the two ".." at the end instead of just one "." 
## This tells Docker to use the parent directory (dotnet-service-bus) as the build context, 
## ...which gives access to both SB.ExampleSender and SB.Utils directories.

## IMPORTANT: --platform linux/amd64 ensures proper OS metadata for ACR/ACA
## AVOID ACR error: use --provenance=false to avoid ACR error "Selected tag uses an invalid operating system"": 
docker image build --provenance=false --platform linux/amd64 -t $img -f Dockerfile ..

## Run with .env variables, and platform for proper metadata (fixes "invalid operating system" error in ACR)
docker container list 
docker container run --name $cnt \
    --platform linux/amd64 -it \
    --env-file .env \
    $img

## Cleanup
docker container rm -f $cnt

## logs: follow logs as they're output
docker container logs -f $cnt



###################################################################

# Azure Container Registry 

# Variables


## RUN: login.azcli for .env variables, login and subscription selection
echo $ENV_VARS
## Create the variables, note ACR can't have dashes in its name, so we need to have removed them previously
acr_repo="${ACR}.azurecr.io/${app}:${tag}"
echo "ACR Repo: $acr_repo"

## Prexisting Container Registry: SEE: aca/aca-acr-env.azcli for details on creation

## Tag locally, then push to ACR as repo
docker tag $img $acr_repo
az acr login -n $ACR -g $RG
docker push $acr_repo

## List the created repositories and tags in ACR
az acr repository list -n $ACR  -o table
az acr repository show-tags -n $ACR --repository $app -o table

