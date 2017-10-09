# ContinuumXmlSlammer 

Alteryx C# plug-in to take in an XML string in a field, and parse it to one record per data element, similar to the JSON Parser tool.

Provided free, courtesy of Continuum (www.continuum.je)



## Introduction

This project is a Windows Visual Studio 2017 project for .Net Framework 4.6.1.  It is a Class Library DLL for use with Alteryx, compiled to x64.

The app implements a tool that processes XML fields into record rows.  Each record holds one data element, and the path to that element.



## Getting Started

You will need Visual Studio 2017 (Community edition is free).  You will need Alteryx Designer.

Download the repo and compile the project using Visual Studio 2017.  You should end up with a DLL in your debug and release folders.  

Assuming you are using the default installation folder, copy the DLL to "C:\Program Files\Alteryx\bin\Plugins".  Alteryx will discover the plug in tool automatically.  The tool will then be found using the Alteryx Search function, and you should search for "XMLSlammer".  The logo is based on the Developer tool series logo (grey cog wheel).  The tool expects an inbound string of XML data.



## Action

The tool will expect to receive a string, formatted as XML.  If there are multiple inbound records, each XML field will be parsed, and presented as sequential records.  The intended use case was for a single inbound record, such as that received from an API via the Download Tool, but the tool will work with multiple inbound records, it just will not separate the results.

The output data creates one record per XML element.  Each element is expressed as a key-value pair, where the key is the position in the XML document, and the value is the data held by that element.  If the element is purely hierarchical, no data value is presented.  The example below illustrates this.

An XML element is a hierarchical node, and it might contain data, child nodes or both (mixed content).  A node may also have any number of attributes.  An attribute can have the same name as a child node, and the parser handles this case.

The output is intentionally modelled on the output of the Alteryx JSON Parse tool.



## Example

Below is a simple XML document, including attributes for Bob and Charlie, and a mixed node for Bob...

```
<Employees>
  <Person>  
    <FirstName>Alice</FirstName>
    <Surname>Avery</Surname>
    <PhoneNumber>0123 456 7890</PhoneNumber>
    <PhoneNumber>0123 456 0789</PhoneNumber>
    <PhoneNumber>0123 456 9028</PhoneNumber>
  </Person>
  <Person>  
    <FirstName nickName="The Bobster">
      Robert
      <NickName>Bob</NickName>
      <NickName>El Boberino</NickName>
    </FirstName>
    <Surname>Bingley</Surname>
    <PhoneNumber>0123 456 1234</PhoneNumber>
    <PhoneNumber>0123 456 2341</PhoneNumber>
  </Person>
  <Person>  
    <FirstName nickName="Charlie">
      Charles
    </FirstName>
    <Surname>Cox</Surname>
    <PhoneNumber>0123 456 7891</PhoneNumber>
  </Person>
</Employees>
```

This gets processed to ...

```
 ------------------------------------------- -----------------
| Path                                      | Value           |
 ------------------------------------------- -----------------
| /Employees	                            |                 |
| /Employees/Person/1                       |                 |
| /Employees/Person/1/FirstName             | Alice           |
| /Employees/Person/1/Surname               | Avery           |
| /Employees/Person/1/PhoneNumber/1         | 0123 456 7890   |
| /Employees/Person/1/PhoneNumber/2         | 0123 456 0789   |
| /Employees/Person/1/PhoneNumber/3         | 0123 456 9028   |
| /Employees/Person/2                       |                 |
| /Employees/Person/2/FirstName             | Robert          |
| /Employees/Person/2/FirstName.nickName    | The Bobster     |
| /Employees/Person/2/FirstName/NickName/1  | Bob             |
| /Employees/Person/2/FirstName/NickName/2  | El Boberino     |
| /Employees/Person/2/Surname               | Bingley         |
| /Employees/Person/2/PhoneNumber/1         | 0123 456 1234   |
| /Employees/Person/2/PhoneNumber/2         | 0123 456 2341   |
| /Employees/Person/3                       |                 |
| /Employees/Person/3/FirstName             | Charles         |
| /Employees/Person/3/FirstName.nickName    | Charlie         |
| /Employees/Person/3/Surname               | Cox             |
| /Employees/Person/3/PhoneNumber           | 0123 456 7891   |
 ------------------------------------------- -----------------
```


Note that in the above example, the default delimiters are used.  This allows the attribute nickName for Robert ("The Bobster") to be differentiated from the child element NickName for Robert ("Bob").  You could change this and use the same delimiter, and the only consequence is that Path keys cease to be unique, you get two rows with the same path.  This may not effect your subsequent processing.  The point is, the output style is configurable.



## Usage

The tool has three settings:

 - XML Field
 - Path Delimiter
 - Attr Delimiter


 
### XML Field

This is expected to be a string field, containing valid XML.  Within the code, no pre-processing is done to this string, I simply throw it at the C# XDocument.Parse() function.  This function will throw an explanatory exception of the XML data is not valid.



### Path Delimiter

Each node in a XML document has a path made up of the node names of it's ancestors, separated by a character, called the delimiter.  The default delimiter is a forward slash, but you could use any character, or indeed string of characters, as the delimiter, if you wish.



### Attr Delimiter

Each node may contain attributes.  Attributes are handled as if they were data child nodes, but presented with a different delimiter to the path delimiter.  You may use the same delimiter if you wish, this causes no problems.  Also, you may use multiple characters (Eg ".Attr." or something).  Attributes must have a unique name in the node, but might have the same name as a child node.  In the example above, Robert has an attribute nickName, and several child nodes for NickName.  



### Author

Steve Hibbert (steve at Continuum dot je), October 2017 (www.continuum.je)



### License

MIT License: Use as you see fit, give credit where applicable.



### Credits and Acknowledgements

 - XML parsing: StackOverflow user Chaviero: https://stackoverflow.com/questions/451950/get-the-xpath-to-an-xelement
 - XML shallow value: M$ https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/how-to-retrieve-the-shallow-value-of-an-element
 - Assistance and guidance: James Dunkerley - Alteryx Ace
