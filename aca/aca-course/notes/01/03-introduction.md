# URL:
- https://www.udemy.com/course/azure-container-apps/learn/lecture/38066590#overview

# Transacript

Let's learn in this video about Azure container apps.

The new service for running containers in Azure within the Azure Portal.

If I select the container section, I can see the services that I can use in order to run containers.

These services have one common objective is that they can run containers in production, but they are

different.

They are different by design.

On the left side, we will find the services that will give you more infrastructure control.

You will manage that infrastructure yourself, like using the Azure Red Hat, OpenShift or the Azure

Kubernetes service, AKs clusters.

On the right side, we will find the services that will enable you to get more out of the developer

productivity, like using the Azure App service with the web app for containers and using Azure Container

Instance ACI or using function app that can also run containers in a serverless infrastructure.

Container apps positioning will be somewhere between the managed Kubernetes clusters and the app service

services.

The objective here is to have a managed environment or a managed service and at the same time get the

value of the developer productivity.

So Azure container apps at the end, that's a fully managed serverless abstraction running on a Kubernetes

infrastructure.

And its purpose is to manage and scale event driven microservices with a consumption based pricing model.

Let's see the, let's say the architecture of a container app.

So it's built on top of the Azure Kubernetes service and.

It will use some open source components like the envoy to manage the proxy or to manage the networking

within the cluster between the services and it will use also keda even for event driven autoscaling

to be able to scale out the containers within the container apps.

It will also use the dapper platform for managing the microservices.

So with container apps, it's like you have a fully managed cluster with the platforms that are installed

on top of that, and then you will be able to deploy your containers and benefit from all of these infrastructures

or platforms.

What you can do now with container apps.

What are the typical applications to deploy into container apps?

First scenario, we will have the microservices because container apps use dapper.

It will be able to handle the microservices architecture for communication between the microservices,

and then the scaling will be done by using keda to scale out from zero to n number of replica and vice

versa.

Second type of applications is the event driven processing.

If we want to trigger an event that will trigger an application to run, that will process a number

of messages within a queue, for example, then we can enable that with container apps and we can use

again keda to look for the number of messages within a queue and then scale out based on that number

of messages.

We can use container apps to expose our services publicly on a public API endpoint and we can benefit

from features like Http traffic split between different versions to say, for example, 80% to the revision

or the version number one and 20% for version number two in order to be able to do blue green testing.

In this case, we can scale based on Http requests for each application.

Another type of applications that could be running within container apps is the background processing.

For scenarios where we want to run continuously running background processes that will, for example,

transform data in a database.

We can do that within container apps and we can scale in this case based on the metrics of CPU and memory.

Let's now see the anatomy of a container apps.

When we create a container app environment, actually we would have the first thing that will be created

is the environment itself.

As we said, container apps is built on top of.

So first of all, we would have that created or the cluster created.

With the creation of container apps.

And of course we'll not see that cluster.

We will not be able to manage it.

But we'll see another resource that is called container app environment.

And here each environment has its own cluster behind the scenes.

Now within that container apps environment, we'll go to deploy our containers.

They will be deployed because here we have an ECS cluster.

They will be deployed within a pod that will live within a revision object and within a pod we can deploy

one or multiple containers and we could also deploy sidecar containers or init containers.

Also, it's worth mentioning here that the only type of workloads supported within container apps are

the Linux containers it does not support yet Windows containers.

Now after deploying our containers, maybe we want to expose these containers.

Through because we are using Kubernetes again, then that will be through an ingress resource.

So we could use either an internal or external ingress, an internal ingress for internal applications

and external ingress for public API endpoint.

And because we might have multiple applications connecting with each other, then we can use the TLS

encryption between these services and we can split the Http requests between different revisions by

using that envoy service that is included and already running within the container app environment.

Applications interacts with external resources like databases, like external services.

And for these reasons, they will need to authenticate and to use some secret and sensitive data like

a connection string or API token.

And for that reason, container apps will support the scenario and will enable you to secure securely

store your sensitive configuration elements within the container apps itself or by using other Azure

services like the Azure Key Vault.

Getting the application logs is very important for the developers to make sure that their application

is behaving correctly as they expect and to get these logs within container apps, we have multiple

options.

Either you don't want get, you want, you don't want to get the container logs, sorry, or either

you want to get the logs and save them into a log analytics workspace.

And container Apps supports the CI CD or the DevOps scenarios where we can use GitHub actions or Azure

DevOps pipelines or any other CI CD tool in order to deploy our containers into the container apps environment.

Let's now move to a demo where we'll go to create a simple container app application within the Azure

Portal.