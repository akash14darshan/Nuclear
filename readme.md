# Nuclear

Nuclear is an open source Multithreaded Reliable UDP Networking Library with tick architecture based server and event based client.

  - Reliable UDP communication to guarantee message delivery with flexible tickrate.
  - Extremely fast with overhead of just 2 bytes per packet.
  - Based on C# Sockets.
  - Included with "Proxy". A data writer to seamlessly convert common data types to memorystream. Refer [this](Docs/Proxy.md) for more info.
  - Example application for Server and Client are included as projects. More info to create a server can be found [here](Docs/Server.md) & client can be found [here](Docs/Client.md)
  
 ## Why Nuclear over other popular frameworks?

  - Completely written in C# without any external library.
  - Open source projects always have an edge over any proprietary framework. 
  - Server tick rate can be varied without any additional code.
  - Follows OOP Inheritance to provide complete abstraction for client and server libraries. Client only has the minimal code required for networking without exposing the server logics.
  - Guaranteed data transmission in UDP with high performance.
  - Utilizes multiple threads to send, receive and handle data requested by multiple connected users.
  - Server can send huge data of any size to the client without any physical limit.
  
  ## Application
  
  - High performance game servers. [Project based on Nuclear networking](https://parabellum.ga/)
  - Instant messaging, Voice calling, Video calling, sending Multimedia.
  - Social networking.
  
  ## Note:
  
  - Sending data above 60000 bytes is an experimental feature. It is not reliable yet.

License
----
MIT

Copyright (c) 2020 Akash Darshan

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.