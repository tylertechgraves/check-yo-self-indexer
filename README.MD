# check-yo-self-indexer Service

This ASP<i>.</i>NET WebApi application provides access to the check-yo-self
Elasticsearch index.  It allows you to create the index, to delete the index,
and to index employees.

## Prerequisites

Instructions for preparing your machine for the check-yo-self app
are located in the check-yo-self-prep-script repo located at the following
url: <https://github.com/tylertechgraves/check-yo-self-prep-script>

## Building the check-yo-self-indexer container

The `dockerize.ps1` and `dockerize.sh` scripts located at the root of this project will build
the required Docker container for this service.  Just choose one to run,
based on the OS you're running.

## Generating the check-yo-self-indexer-sdk NuGet

The `packNugets.ps1` and `packNugets.sh` scripts located in the SDK project's
build-scripts folder will generate an SDK NuGet that other developers
can consume to more easily interact with this API.  Just choose one to run,
based on the OS you're running.

## Accessing the check-yo-self-indexer Swagger page

Once the check-yo-self-indexer Docker container is listening on port 80,
you can navigate to the app in a browser by going to the following URL:

[http://localhost:5002/swagger](http://localhost:5002/swagger)
