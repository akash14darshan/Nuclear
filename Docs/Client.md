# Creating Client Instance

[Example project](../Nuclear.Client.Application/) has been included as a reference.

### Steps
- Create a new project and reference it to "Nuclear.Client.dll" & "Nuclear.Common.dll"
- Create a class inheriting from the abstract class "NuclearClient" initialized with a constructor (string hostserverip,int port). DNS support is not supported currently.
- (Optional) Start any init function at the constructor
- Implement all the overrides as the parent class is abstract type.

#### Establishing Connection
- Connect() function needs to be called in the child class. Preferrably from the constructor.

#### Sending Data to server
```sh
byte opCode;
MemoryStream data;
SendEvent(opCode,data);
```

### Overrides Explained

#### ```OnConnected()```
The following override function is executed when the client is connected to server successfully

#### ```OnDisconnected()```
The following override function is executed when the client loses its connection to the server.

#### ```OnException(Exception ex)```
The following override function is executed when there is an internal exception.

#### ```OnDisableAndDisconnect()```
The following override function is executed when the server disconnects forcibly disconnects the client.

#### ```OnEvent(byte opCode,byte[] data)```
The following override function is executed when the client receives data from the server

### Exposed Fields
#### ```base.Ping```
Following field which is accessible from any child of the class "NuclearClient " which represents the time taken for data to travel from server to client.

#### ```base.PendingRequests```
Following field which is accessible from any child of the class "NuclearClient " which represents the pending request from server yet to be processed by the client. This can be handy when the server has an extremely high tickrate and the client cannot keep up. A framework can be implemented to discard packets at extreme cases which wouldn't affect the current status of the application.
