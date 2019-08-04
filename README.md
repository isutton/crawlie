# Crawlie [![Build Status](https://travis-ci.org/isutton/crawlie.svg?branch=master)](https://travis-ci.org/isutton/crawlie)

WIP.

## Running Crawlie

### Using `dotnet`

Running *Crawlie.Server*:

```shell
$ dotnet run --project Crawlie.Server/Crawlie.Server.csproj
```

Running *Crawlie.Client.App*:

```shell
$ dotnet run                                               \
    --project Crawlie.Client.App/Crawlie.Client.App.csproj \
    -- /RunnerOptions:Url=https://www.redhat.com/en
```

**Note:** There is a space between the double dash and `/RunnerOptions` in the 
snippet above; that is used as delimiter by `dotnet` (and other applications) 
to ignore any arguments from that point on and proxy it to the application it 
is hosting.

### Using Docker (WIP)

Running *Crawlie.Server*:

```shell
$ sudo docker run -d -p 5001:80 crawlie-server:latest
```

Running *Crawlie.Client.App*:

```shell
$ sudo docker run -it --rm crawlie-client:latest /RunnerOptions:Url=https://www.redhat.com/en
```

## Architecture
    
WIP.    

## Known Issues

WIP.

## License

WIP.

