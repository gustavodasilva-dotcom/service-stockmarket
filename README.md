# service-stockmarket

Mainly, I use .NET Core 5 for my projects. But I think it's important to know (or, at least, be familiarized) with the .NET Framework.

So, the main reason for this project was to create a Windows Service using the tools and resources of the .NET Framework.

In the end, here is the final project: a Windows Service, that makes requests to a remote API (the API is the Finnhub [https://finnhub.io/docs/api/], an API about financial information), handles the data (companies profile, stock symbols information, etc.), and, if it doesn't exists, inserts the data at the database.