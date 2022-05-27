# Guidance Example
This small example demonstrates how it is possible to implement a "Guidance" endpoint on any service group. 
The purpose of the guidance endpoint is to provide additonal information, which can help build a UI.

Guidance is provided in the form of an OAS document. The OAS document is dynamic, depending on what the caller wants to achieve.

In this example we are trying to create a "User", but under three different circumstances. Depending on the circumstance:
* Some fields are relevant or not
* Some fields are required or not
* Some enumerations have fewer or more possible values.

The rules are outlined below. They depend on a combination of UseCase and UserRole.

| Field						| SaxoCreatingClient	| IBCreatingUser (Retail User)	| IBCreatingUser (other) | WLCCreatingClient | Default  |
| ------------------------- | --------------------- | ----------------------------- | ---------------------- | ----------------- | -------- | 
| UserId					| Required				| Required						| Required				 | Required			 | Required |
| UserName					| Required				| Required						| Required				 | Optional			 | Optional |
| Email						| Required				| Required						| Required				 | N/A				 | Optional |
| Address					| Required				| Required						| N/A					 | N/A				 | Optional |
| Zipcode					| Required				| Required						| N/A					 | N/A				 | Optional |
| IdentificationType		| DL,PassPort			| Passport						| N/A					 | N/A				 | Optional |
| Roles						| RetailUser			| RetailUser					| Ts,Tm,Cs,Cm			 | RetailUser		 | Optional |


To try it out call the POST /users/guidance/{useCaseId} with correct combination of useCaseId, and potentially the value of the userRole in the post body