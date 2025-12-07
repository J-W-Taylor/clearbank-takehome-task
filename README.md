### Overview

During the coding task, I focussed on improving correctness, testability, and adherence to SOLID principles within the payment processing logic.
The main refactoring centred on decoupling data access, improving the structure of payment validation, and ensuring funds moved between accounts
correctly.

### Assumptions

The following assumptions were made regarding the desired behaviour of the code:
- The actual databases behind 'AccountDataStore' and 'BackupAccountDataStore' are kept in sync by an external mechanism
  (i.e. we do not need to write updates to both to keep them in sync from this service).
- The only difference in the implementation of the original 'AccountDataStore' and 'BackupAccountDataStore' classes was the connection string.
- The desired use of the 'Backup' data store is to be read from/written to only in the event that reading/writing to the primary data store fails.
- The actual implementation for 'GetAccount' in the AccountDataStore will throw an exception if connecting to the database fails, and return 'null'
  if it succeeds but no account can be found.
- The actual implementation for 'UpdateAccount' will throw an exception if connecting to the database fails.
- There are only, and will only ever be, 2 Account Data Stores.
- The intended functionality of the MakePayment method is to move the amount of money specified in the request's 'Amount' field from the
  Debtor's account into the Creditor's account.

### Changes made and rationale

The following changes were completed within the 3 hour window provided
- AccountDataStore and BackupAccountDataStore were combined into a single class with a private 'connectionstring' property.
	- This is due to the above assumption that the connection string is the only difference between the implementation, and meant we removed
	  duplicated code.
- Added an interface IAccountDataStore.
	- This is to better cater for dependency injection - services depending on instances of the AccountDataStore should depend on the abstraction.
- Added IDataStoreFactory and DataStoreFactory.
	- This was to move the responsibility of instantiating IAccountDataStore away from the MakePayment service.
	- These factory classes also currently handle the responsibility of reading data from the App Configuration.
	- This helps MakePayment adhere to the Single Responsibility principle.
	- The use of an interface again helps cater for dependency injection.
- Added logic to retrieve Creditor account details as well as Debtor account details.
	- This was based on the assumption that we want to move money from one account to the other - we need knowledge of both accounts to do this.
- Refactored reading/writing of data to the data stores to always try primary data store first, then fallback to backup data store.
	- Previously only one data store or the other would be instantiated, which meant we couldn't 'try primary and fall back to the backup' as per
	  the assumption above.
- Added 'FailureMessage' field to the MakePaymentResponse object
	- In the case of an error, this provides a helpful error message to the sender of the request as to why their request failed.
- Added error handling for when either account cannot be found in the data store.
	- The service should not add/remove money from one account if the other cannot be found.
- Refactored the logic for validating a payment based on the PaymentScheme to use a Strategy Pattern
	- This removed the Switch statement (which violated the open/closed principle of SOLID) out of the MakePayment service and into separate classes
- Added error handling for an invalid payment scheme being provided
	- This safeguards against the case where a new payment scheme is created and we forget to create a handler for that payment scheme
- Added an error message in the event that the Payment's validation fails
- Added unit tests for:
	- Retrieving an account if one exists.
	- Backup data store is called if primary data store connection fails.
	- Error handling if no account is found.
	- Error if invalid payment scheme provided.
	- Validation of each payment scheme.
	- Account balances for debtor/creditor decrease/increase as expected.

### Areas for future improvement

With more time to work on the project, I would consider the following changes:
- Currently the PaymentService still has knowledge of how many AccountDataStores there are and what order to try them in. I would ideally
  wrap the DataStoreFactory in a dedicated 'failover' class whose responsibility it is to know this information. The PaymentService would then
  call e.g. _failover.GetAccount(); and not need to know how many databases would be tried before the desired data was retrieved.
- Handling of concurrency - what happens if the Debtor account is updated, then the Creditor account fails to update? Ideally we would only want
  to commit both transactions if we knew they would both succeed.
- Currently there are hardcoded exception strings in the PaymentService that are duplicated in the unit tests. This makes the tests flaky - a
  change in the wording results in the tests failing. To resolve this, these strings should be extracted to a class of constants that can be
  referenced by both
- Better error handling
	- What happens if the connection to the backup data store fails? Currently an unhandled exception would be thrown,
	  which would not provide the caller with helpful information on why the request failed.
	- When a payment request's validation fails, it could be because the account doesn't allow the request payment scheme, it could be
	  due to insufficient funds, or an invalid account status. Due to the refactor the PaymentService no longer knows WHY validation fails so
	  cannot provide that context in its response. The 'PaymentRule' classes could be updated to return a custom object that includes the
	  error reason, which could then be returned from MakePayment.