# GHF

Create a WebAPI with .net 6 , which retrieves the information of a GitHub User and creates a new Contact or updates an existing contact in Freshdesk, using their respective APIs. 

Use GitHub Rest API v3. Documentation is available at https://developer.github.com/v3/

Use Freshdesk API v2. Documentation is available at

Freshdesk https://developers.freshdesk.com/api/

OPTIONAL API should be protected with JWT/Bearer token

API should expose endpoint which expect GitHub user login (username) and Freshdesk subdomain as parameter.

For authentication assume GitHub personal access token is given in GITHUB__TOKEN environmental variable and Freshdesk API key is given in FRESHDESK__TOKEN environmental variable.

Transfer all compatible fields from the GitHub User to the Freshdesk Contact by your judgment

Provide unit tests for the main program functionality.

You may also use any nugets which will help you with the task, such as requests, mock, swagger and etc, except for API clients for Freshdesk or GitHub.

OPTIONAL Create dockerfile for build an image.

While you may create trial accounts in GitHub and Freshdesk, this is not a requirement. You can use the examples from the documentation as test data.
