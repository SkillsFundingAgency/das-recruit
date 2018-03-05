![Build badge](https://sfa-gov-uk.visualstudio.com/_apis/public/build/definitions/c39e0c0b-7aff-4606-b160-3566f3bbce23/788/badge)

# Employer Recruit (Alpha)

This solution represents the Employer Recruit code base currently in alpha.

## Developer setup

### Requirements

In order to run this solution locally you will need the following installed:

* [.NET Core SDK >= 2.1.4](https://www.microsoft.com/net/download/)
* (VS Code Only) [C# Extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
* [Docker for X](https://docs.docker.com/install/#supported-platforms)

### Environment Setup

The default development environment uses docker containers to host it's dependencies.

* Redis
* Elasticsearch
* Logstash
* MongoDb

On first setup run the following command from _**/setup/containers/**_ to create the docker container images:

`docker-compose build`

To start the containers run:

`docker-compose up -d`

You can view the state of the running containers using:

`docker ps -a`


### Running

* Open command prompt and change directory to _**/src/jobs/Recruit.Vacancies.Jobs/**_
* Run `dotnet run` to start up the **Webjob**
* Open second command prompt and change directory to _**/src/Employer.Web/**_
* Run `dotnet run` to start up the **Website**
* Browse to `http://localhost:5020/accounts/abc/dashboard`

### Application logs
Application logs are logged to [Elasticsearch](https://www.elastic.co/products/elasticsearch) and can be viewed using [Kibana](https://www.elastic.co/products/kibana) at http://localhost:5601

### Development 

#### Website

* Open solution _**/src/Esfa.Recruit.Employer.sln**_

#### Webjobs

* Open project _**/src/jobs/Recruit.Vacancies.Jobs/Recruit.Vacancies.Jobs.csproj**_


## License

Licensed under the [MIT license](LICENSE)
