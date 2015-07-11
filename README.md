# Parse-and-Facebook-Unity3d
Functional login-logout / send data / receive data / invite / share ||
Why I made this API?
Unity3d does use .NET 4.5.1 only in Universal Windows Applications ( Windows Store ) in all the other platform supports .NET 2.0/3.5

What does that mean?

http://www.parse.com/docs/dotnet/api/Index.html

You cannot use something like that:

{"ParseQuery query = new ParseQuery("MyClass");
IEnumerable<ParseObject> result = ---await--- query.FindAsync();"}
=> .Result will be blocking an async task somewhere.
=> Your only option is use a .ContinueWith and fire an event or a delegate inside of it.
Now you can use PutOnGameObject.MyKindOfThread.ExecuteOnMainThreadAtFirstUpdate(() => { }); inside of a .ContinueWith
to fix that problem, see the exemple.
