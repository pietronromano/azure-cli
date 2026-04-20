# Search for APIs and operations using sql-data-source resolver in a specific APIM instance

**NOTE**: Initially tried to do this for all subscriptions and APIM instances, but didn't work well due to the large number of APIM instances and the fact that many subscriptions are not accessible. 

So, switched to a more targeted approach where we specify the APIM instance and resource group directly.

```bash
./search-apim-sql-resolver.sh apim-..	...rg-apim..	sub...
```
