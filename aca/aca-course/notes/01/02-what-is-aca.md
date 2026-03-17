# URL:
- https://www.udemy.com/course/azure-container-apps/learn/lecture/52176011#overview

# Transacript
Let's learn in this Lightboard session about Azure Container Apps.

Azure Container Apps is a new service on Azure for running containers based on serverless infrastructure.

Azure Container Apps sits between ACI Azure Container Instance and AKS Azure Kubernetes Service.

Azure Container Apps will benefit from the ease of managing an ACI instance, and it will take some

of the powerful features provided with AKS, like being able to deploy multiple applications or microservices

scaling based on external metrics other than CPU and memory, and being able to split the traffic between

different revisions or different versions of the application.

Let's see how all of these components work together on the Lightboard.

Let's learn in this Lightboard session about the architecture and the features of Azure Container Apps.

Azure Container Apps behind the scenes.

Let's learn in this session about the architecture and the features of Azure Container Apps.

Azure Container Apps is serverless service for running containers, but behind the scenes today it is

using AKS cluster.

Azure Kubernetes service.

In order to be able to deploy those containers, and within that AKS, we would have some open source

components that will be leveraged in order to provide more features to the container apps like using

Keda for event driven autoscaling to autoscale the applications, and the dapper in order dapper platform

in order to provide features for if we are implementing microservices, and also using envoy, which

is reverse proxy for routing the traffic between the different components of the application.

So when we create an Azure Container apps in Azure, at first we would actually create the environment,

the environment.

If you are familiar with Azure App Service, you are familiar with the plan with the App Service plan.

And then within that plan we deploy the apps themselves.

It's the same concept within Azure Container apps.

First of all, we would have the environment within the Azure portal.

When you create container apps, it will ask you first to create the environment for your application.

And then within the environment we can go to deploy one or multiple container apps as we do with Azure

App Service environment.

So let's say for example here I would have first front end application and the back end application.

So I would have here the boundary for the first application.

This is going to be my container apps.

We can somehow compare this as a namespace in Kubernetes.

It's not exactly that, but just for comparison it's like namespace where you can deploy one or multiple

pods.

The pods will be translated as a revision in container apps, and within the revision we would have

the unit of deployment, which is going to be the container itself.

So I might have here my container which will be deployed in here.

And of course I can scale out those containers either using CPU and memory or using the events provided

by Keda, like for example, by watching the number of messages within EQ and then scaling based on

those number of messages.

So those containers might be one or many.

It's up to us to scale them out.

Today we can go up to 100 instances of those containers.

Now when deploying when developing the application, you might have a new version of the application.

So I will create a new I will build a new container instance.

And then I want to deploy that new version.

How we can deploy do that with container app.

So either you can go to override the existing container and route the traffic to that new container.

Or even better, you can go to define a second container within a second revision.

So for that earlier we have talked about this revision.

So revision is a logical boundary for the containers themselves.

So I might have a revision let's say revision one here.

And then when I have a second application or a second version of my application I can go to create a

revision number two.

Within that revision number two, I can go to deploy the second version of my container.

So for example, if this was container v1 then here I can go to deploy container v2.

And then I can go to scale it independently.

This concept of revision here is very very interesting because later if I want to consume those containers

or those applications deployed here, I can use another concept in container apps, which is the ingress

and this traffic split.

So what I can do here, let's say I would have the URL for my application which will be exposed somewhere

here.

And that could be internal or external endpoint.

Let's say I have that endpoint at the end.

Let's say this is an internal endpoint.

Then from here I can go to connect to either the first revision or also I can go to connect to that

second revision.

And they can go to say here, I want to get 20% of the traffic routed to the V1, for example, and

80% routed to the V2, for example.

So that's the traffic split for the application.

And that's will be a performance thanks to using envoy.

And with that way we can have canary deployment between V1 of my application and V2.

So here we are still using the same application.

Let's say this is my backend application for example.

And so we might have another container app within our environment that will go to consume that backend

application.

So let's go to design it here.

So again in this second app I would have the revision and also the containers.

The revision actually will be created by default when we create or when we create.

When we deploy the new container.

And all the containers will be able to scale independently from each other within that same environment.

So let's say this is my front end application that will go to connect to the backend endpoint here that

will be exposed on internal.

It means it will be exposed on a private IP.

Okay.

So let's go to mention that private IP or private FQDN and this front end will be able then to reach

to that endpoint.

And behind the scenes we can imagine this will be within the same virtual network within the Azure VNet.

So the traffic will not leave that AKS VNet.

Now when we expose those containers like a back end, then this one, the front end can consume it by using environment variables where it will need to get that endpoint.

So each environment or each container app would have a set of environment variable.

And it will also expose its own port numbers and so on.

So we have control over those operations in addition to that actually within a revision or within the pod.

As with the pod in Kubernetes, we can deploy not only the container, but we can also deploy init containers and sidecars.

We can do the same thing with container apps.

So we might have here some init container that will do some, uh, job before running the main container.

And you might also have proxy or proxy sidecar that can perform some network operations.

If, for example, we are here using the traffic, uh, splitting the traffic, that could be done through

the proxy sidecar.

The proxy sidecar could be also used in order to have mtls enabled between the for the traffic between

the different containers.

So this traffic from here, from the front end to the back end could be enabled actually through Mtls.

And we can have mtls either with dapper by using dapper or without using dapper.

So we just enable that feature.

So here I talked about exposing a service internally within the environment itself.

That service is only accessible within the environment for applications inside there.

Now we can also expose services outside or external services that will be exposed to the end users that

can connect on the internet.

So for that, we can use another type of service that is called the external service within container

apps.

So we can expose another endpoint.

Let's say this is an external endpoint.

And then the end users will be able to connect there.

So they will be using their web applications or browsers in order to connect to this endpoint.

And of course here we can leverage some other Azure services for enabling more secure connection to

this external endpoint.

Like for example, we can use the Azure apps, the Azure App Gateway or front door or API management.

So all of those have integration with the container apps.

So then the user, instead of connecting here directly, it might be able to connect to one of these

components instead of connecting directly.

So this is for securing the incoming or for securing the ingress traffic.

Now what about the egress traffic or the traffic that we leave the cluster or that we leave the container

apps because we might have here containers connecting to external endpoints or external services outside

of this environment.

So in that case we have another feature within container apps that is the UDR mode.

UDR means user defined routing.

And with this feature, you can have all the egress traffic that will be routed into an Azure Firewall,

for example.

And within the firewall you can go to control that egress traffic to allow or deny some traffic to some

specific endpoints.

I hope this was helpful.

Thank you.