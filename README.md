# Makelaars
This an assignment repository. It was built using:
* Visual Studio 2019
* DotNet Core 3.0 and C#
* NUnit for tests

The **Makelaar** application lists the top 10 makelaars for offer counts in:
* Amsterdam
* Amsterdam with a Garden

The API that returns the offers is rate limited and has a maximum page size of 25. These factors come into play when fetching data, and the application attempts to provide some feedback whenever offers are retrieved and when rate limiting is active. The application might seem unresponsive while rate limiting is active, but it is continually retrying in the background.

## Run the application
`dotnet run --project .\Makelaars`

## Test the application
`dotnet test`

## Configuration
The application can be configured with these environment variables:
* `API_KEY` - API Key to use for API calls
* `API_URL` - Base URL to use for API calls
* `PAGE_SIZE` - Default page size to use for API calls (max: 25)