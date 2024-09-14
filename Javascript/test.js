//Invoke http request with ActiveXObject
var xhr = new ActiveXObject("MSXML2.XMLHTTP.3.0");
xhr.open("GET", "https://www.google.com", false);
xhr.send();

//print response
WScript.Echo(xhr.responseText);
