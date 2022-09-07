Dor bivas 315557850 
Submission task: Student C#/.NET Backend Developer test 4.22
Implement a caching server

~Server managed by fully associative LRU cache ~
Please forgive a minor change ~
input line format: 
set cache: "set xxxx(Key) xxxx(Size in bytes) xx..xx(Data)
eg. set email 12 dr@gmail.com
(size must be correspond to the amount of characters (later will be translated to bytes).
server responde: OK (will add or update new data and remove if needed) \ MISSING (if failed)

get cache: " get xxxx(Key)
eg. get email
server responde: OK Size Data \ MISSING (if didnt find)

attacments:
* sln
* test file
* readme
