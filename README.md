# JumpCloud
QA Exercise

Main bugs found:
1. It takes 5 sec for the job ID to return when submitting to hash endpoint. Specification says it should return immediately. 
2. The /stats request always returns "AverageTime" as 0


Test Cases:

Test Case 1 (POST endpoint):
AC covered:
A POST to /hash should accept a password.
It should return a job identifier immediately
It should then wait 5 seconds and compute the password hash.
The hashing algorithm should be SHA512.

Test Steps:
1. Launch application.
2. Initiate a valid HTTP Post request to the hash endpoint, using the port specified in the environment variable (ie where port = 8088 curl -X POST -H "application/json" -d "{\"password\":\"khuron\"}" http://127.0.0.1:8088/hash).


Expected Results:
1. Application launches successfully and waits for further action.
2. Job identifier is returned immediately. After 5 seconds, the password is hashed(SHA512) and will be returnable when requested. Note the job identifier starts at 1 and will increment with every request while the application is running. 

Test Results:
Failure: Job identifier is NOT returned immediately, it is returned after 5 seconds. 
=======================================================================================
Test Case 2 (POST endpoint):
AC covered:
It should answer on the PORT specified in the PORT environment variable.

Test Steps:
1. Launch application
2. Initiate a HTTP Post request to the hash endpoint, but do NOT use the port specified in the environment variable (ie where port = 8088 curl -X POST -H "application/json" -d "{\"password\":\"khuron\"}" http://127.0.0.1:8080/hash)

Expected Results:
1. Application launches successfully and waits for further action.
2. Connection is refused

Test Results:
Pass
=======================================================================================
Test Case 3 (GET hash endpoint):
AC covered:
A GET to /hash should accept a job identifier. 
It should return the base64 encoded password hash for the corresponding POST request.

Test Steps:
1. Launch application.
2. Initiate 3 valid HTTP Post requests to the hash endpoint, using the port specified in the environment variable, and different passwords for each request (ie where port = 8088 curl -X POST -H "application/json" -d "{\"password\":\"khuron\"}" http://127.0.0.1:8088/hash).
3. Initiate a GET request to the hash end point for the first request made (ie curl -H "application/json" http://127.0.0.1:8088/hash/1).
4. Initiate a GET request to the hash end point for the second request made (ie curl -H "application/json" http://127.0.0.1:8088/hash/2).
5. Initiate a GET request to the hash end point for the third request made (ie curl -H "application/json" http://127.0.0.1:8088/hash/3).
6. Initiate a final Get request, this time reuse one of the passwords from a previous step.

Expected Results:
1. Application launches successfully and waits for further action.
2. Job identifier is returned immediately. Take note of the identifier for furture steps.
3. Password hash is returned. This value should be unique when compared to the rest of the hashes for this test.
4. Password hash is returned. This value should be unique when compared to the rest of the hashes for this test.
5. Password hash is returned. This value should be unique when compared to the rest of the hashes for this test.
6. Password hash is returned and it matches the hash returned for the other password that was reused.

Test Results:
Pass
=======================================================================================
Test Case 4 (GET hash endpoint):
AC covered:
A GET to /hash should accept a job identifier. 
It should return the base64 encoded password hash for the corresponding POST request.
NOTE: No error handling was specified > Something to clarify/ask product owner

Test Steps:
1. Launch application.
2. Initiate a GET request to the hash end point for any integer job identifier (ie curl -H "application/json" http://127.0.0.1:8088/hash/3).
3. Initiate a GET request to the hash end point for any string job identifier (ie curl -H "application/json" http://127.0.0.1:8088/hash/taco).

Expected Results:
1. Application launches successfully and waits for further action.
2. "Hash not found" message should return since nothing has been posted to the application and the job identifier does not exist. Application should continue to run and respond to valid requests.
3. Request fails due to inability to parse the string and "invalid syntax" is returned. Application should continue to run and respond to valid requests. 

Test Results:
Pass
=======================================================================================
Test Case 5 (GET stats endpoint):
AC covered:
A GET to /stats should accept no data.
It should return a JSON data structure for the total hash requests since the server started and the average time of a hash request in milliseconds.

Test Steps:
1. Launch application.
2. Make several POST requests.
3. Initiate a GET request to the stats endpoint (ie curl http://127.0.0.1:8088/stats)

Expected Results:
1. Application launches successfully and waits for further action.
2. Requests are successful, job identifiers are returned.
3. JSON is returned containing the total hash requests since the application started and the average time of the hash requests in miliseconds.

Test Results:
Failure: "AverageTime" is always 0
=======================================================================================
Test Case 6 (GET stats endpoint):
AC covered:
A GET to /stats should accept no data.
It should return a JSON data structure for the total hash requests since the server started and the average time of a hash request in milliseconds.

Test Steps:
1. Launch application.
2. Make several POST requests.
3. Initiate a GET request to the stats endpoint and specify additional URL parameter (ie curl http://127.0.0.1:8088/stats/1).

Expected Results:
1. Application launches successfully and waits for further action.
2. Requests are successful, job identifiers are returned.
3. 404 is returned.

Test Results:
Pass
=======================================================================================
Test case 7 (Shutdown behavior)
AC covered:
The software should support a graceful shutdown request.
It should allow any in-flight password hashing to complete
It should reject any new requests, respond with a 200 and  shutdown.

Test Steps:
1. Launch application.
2. Initate a shutdown request (ie curl -X POST -d "shutdown" http://127.0.0.1:8088/hash)

Expected Results:
1. Application launches successfully and waits for further action.
2. Request for shutdown is completed.

Test Results:
Pass
=======================================================================================
Test case 8 (Shutdown behavior)
AC covered:
The software should support a graceful shutdown request.
It should allow any in-flight password hashing to complete
It should reject any new requests, respond with a 200 and  shutdown.

Test Steps:
1. Launch application.
2. Initiate several password hash POSTs.
3. While the POSTs are processing, initate a shutdown request (ie curl -X POST -d "shutdown" http://127.0.0.1:8088/hash)

Expected Results:
1. Application launches successfully and waits for further action.
2. POST requests begin to process.
3. Password hashing POST requests complete (return job identifier), and then the application stops.

Test Results:
Pass
=======================================================================================
Test case 9 (Shutdown behavior)
AC covered:
The software should support a graceful shutdown request.
It should allow any in-flight password hashing to complete
It should reject any new requests, respond with a 200 and  shutdown.

Test Steps:
1. Launch application.
2. Initate a shutdown request (ie curl -X POST -d "shutdown" http://127.0.0.1:8088/hash)
3. Initate a POST request to the hash endpoint while server is shutting down.

Expected Results:
1. Application launches successfully and waits for further action.
2. Shutdown begins
3. New request is rejected, a 200 is returned and application shuts down. 

Test Results:
Pass
=======================================================================================
Test case 9 (Multiple requests)

AC covered:
The software should be able to process multiple connections simultaneously

Test Steps:
1. Launch application.
2. Initiate 4 password hash POSTs. Make sure requests have unique passwords and are initiated at the same time
3. Check the hashes of the passwords for the previous step.


Expected Results:
1. Application launches successfully and waits for further action.
2. POST requests are processed simultaneously.
3. Password hashes match the job identifier they were associated with upon POST.

Test Results:
Pass
