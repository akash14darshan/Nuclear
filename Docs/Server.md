# Creating Server Instance

[Example project](../Nuclear.Server.Application/) has been included as a reference.

### Steps
- Create a new project and reference it to "Nuclear.Server.dll" & "Nuclear.Common.dll"
- Create a class inheriting from the abstract class "NuclearServer" initialized with a constructor (int port,int tickrate)
- (Optional) Start any init function at the constructor
- Implement all the overrides as the parent class is abstract type.
- The created class is the server interface. It needs to be created by a controller class with port and tickrate in the constructor to start functioning.

## Note:
Peer is the class denoting any connected user to the client.
### Overrides Explained

#### ```OnPeerJoin(Peer peer)```
The following override function is executed when any new user makes connection to the server.

#### ```OnPeerLeave(Peer peer)```
The following override function is executed when any Peer gets disconnected from the server.

#### ```OnTick()```
The following override function is executed at every tick. Any function which requires execution at tick needs to be handled in here.

#### ```OnException()```
The following override function is executed when there's any exception occuring at Tick or internally.

#### ```OnEvent(Peer peer, byte Opcode,MemoryStream stream)```
The following override function is executed when server receives data from a client.

### Exposed Fields

#### ```Peer.SendEvent(byte[] data)```
Accessible through any Peer object which sends the data to the client. Client expects the first byte to contain the OpCode.

#### ```protected List<Peer> Peers```
Accessible through any child class of NuclearServer. This contains the list of active Peers connected to the server.

#### ```protected PeerLock```
Accessible through any child class of NuclearServer. This object can be used to lock incase there is a non thread safe code getting executed.