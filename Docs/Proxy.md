# How to serialize datatypes:

### Supported datatypes:
  - bool,byte & byte[]
  - string, list, dictionary, enum, datetime
  - unsigned int16, int16, int32, int64, float, decimal
  

For instance if you desire to transmit a bool and string
### Sender
```sh
string testString = "HelloWorld";
bool testBool = true;
using(MemoryStream stream = new MemoryStream())
{
    StringProxy.Serialize(stream,testString);
    BooleanProxy.Serialize(stream,testBool);
    peer.SendEvent(stream.ToArray());
}
```

### Receiver
```sh
byte[] receivedData; //should be byte data received
string receivedString;
string receivedBool;
using(MemoryStream stream = new MemoryStream(receivedData))
{
    receivedString = StringProxy.Deserialize(stream);
    receivedBool = BooleanProxy.Deserialize(stream);
}
```

### Serializing and Deserializing custom class and struct
##### Example serialization/deserialization
```sh
public class Contact
{
    public string Name { get;set; }
    public string Address { get;set; }
    public int Number { get;set; }
    public byte[] Picture { get;set; }
    
    public static void Serialize(Stream stream, Contact instance)
    {
        StringProxy.Serialize(stream,instance.Name);
        StringProxy.Serialize(stream,instance.Address);
        Int32Proxy.Serialize(stream,instance.Number);
        ByteProxy.SerializeArray(stream,instance.Picture); 
        //Serialize array is only supported by byte[]
        //Use List incase of other datatypes
    }
    
    public static Contact Deserialize(Stream stream)
    {
        return new Contact{
            Name = StringProxy.Deserialize(stream),
            Address = StringProxy.Deserialize(stream),
            Number = Int32Proxy.Deserialize(stream),
            Picture = ByteProxy.DeserializeArray(stream)
        };
    }
}
```

##### Example usage
```sh
//Sender
using(MemoryStream stream = new MemoryStream())
{
    byte opCode = 1;
    Contact contact = new Contact{
        Name = "Akash",
        Address = "India",
        Number = 100,
        Picture = new byte[5]
    };
    Contact.Serialize(stream,contact);
    SendEvent(opCode,stream.ToArray());
}

//Receiver
byte[] receivedData;

using(MemoryStream stream = new MemoryStream(receivedData))
{
    Contact contact = Contact.Deserialize(stream);
}
```

### Serializing and Deserializing List
```sh
List<int> IntegerCollection; 
List<Contact> PhoneBook;

//Sender
byte opCode = 1;
using(MemoryStream stream = new MemoryStream())
{
    ListProxy<int>.Serialize(stream, IntegerCollection, Int32Proxy.Serialize);
    ListProxy<Contact>.Serialize(stream, PhoneBook, Contact.Serialize);
    SendEvent(opCode,stream.ToArray());
}

//Receiver
byte[] receivedData;
using(MemoryStream stream = new MemoryStream(receivedData))
{
    List<int> intCollection = ListProxy<int>.Deserialize(stream, Int32Proxy.Deserialize);
    List<Contact> phoneBook = ListProxy<int>.Deserialize(stream, Contact.Deserialize);
}
```
