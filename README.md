# Crawlie [![Build Status](https://travis-ci.org/isutton/crawlie.svg?branch=master)](https://travis-ci.org/isutton/crawlie)

*Crawlie* is an implementation of a Crawler Platform. In short, it receives a 
request to collect all links for a given URL, collecting the information on a 
separate logical process for further collection. Additionally, there is a 
client that interacts with the Crawlie Server to build a Sitemap of a given 
URL.

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

### Using Docker

Building `crawlie-server` and `crawlie-client` images:

```shell
$ make
```

Running *Crawlie.Server*:

```shell
$ sudo docker run -d -p 5001:5001 --name crawlile-server crawlie-server:latest
```

Running *Crawlie.Client.App*:

```shell
$ sudo docker run -it --rm --network=host crawlie-client:latest /RunnerOptions:Url=https://www.redhat.com/en
```

## Architecture

The *Crawlie Server* is an ASP.NET Core application, where two endpoints are 
currently configured: `POST /api/CrawlerJob` and 
`GET /api/CrawlerJob?jobId=<ENCODED_URL>`.

Those endpoints interact with a *Crawler Job Repository*, both to add a 
request for a given URL, or to consult the status of given URL.

The processing of those Crawler Jobs are delegated to specialized workers in 
background. Their job is to collect all the links the given URL refers to. 
Once their work is finished, the job they finished is marked as complete, 
indicating to an external actor that the job has been finished and the result 
of their operation can be found in the job itself.

### `POST  /api/CrawlerJob`

Receives an `application/json` body like the following:

```json
{
  "Url": "https://www.redhat.com/en"
}
```

Returns an `application/json` payload like the following, indicating the 
Crawler Job has been accepted:

```json
{
  "Id": "https://www.redhat.com/en",
  "Status": 2
}
```

Calls to this endpoint are idempotent once the Crawler Job has been accepted. 
This means that calling this endpoint will return any existing results to the 
client if data is available.

### `GET /api/CrawlerJob?jobId=<URL>`

Returns **Not Found** in the case *jobId* is unknown, otherwise returns a 
Crawler Job Response payload:

```json
{
  "Id": "https://www.redhat.com/en",
  "Status": 2
}
```

Or as anoher example, a payload representing a complete Crawler Job:

```json
{
  "Id": "https://www.redhat.com/en",
  "Status": 3,
  "Result": ["https://www.redhat.com/summit"]
}
```

### Crawler Background Service

Running as its own logical process, the Crawler Background Service workers
consume items enqueued by the endpoints through a queue (in the case of the
current implementation, an in memory ConcurrentQueue instance is being used).

Those workers are responsible for harvesting all links found in the document
available at the given URL and, once finished, update the Crawler Job Status
field to complete and store the harvested URLs.

## Known Limitations

- Only the links found on the document itself are being returned. To
properly achieve the objective to compose a SiteMap, those URLs should be 
harvested themselves.  
I believe the structure in place should allow such feature, but the hard work 
lies in finding the __right high-level data-structure__ (for example, to not 
allow infinite recursion in URLs that point each other) and also reuse the 
existing worker pipeline. The current design isolates this problem from the 
rest of the system.  

- Once a URL has been harvested, it won't be re-processed. This is a matter
of either adding another endpoint or adjusting the existing 
`POST /api/CrawlerJob` interface to accomodate this feature.

- Type names are a little bit confusing, need to rename some classes to better 
represent what they actually are (CrawlerJobResponse should be called 
CrawlerJobStatusResponse as an example).

