# Passado
Platform Agnostic Strongly-typed Sql over ADO.net

Select:
```
var rows = qb.Select(q => q.From(t => t.Users)
                           .Join(t => t.Addresses)
                           .Select(t => new { FirstName = t.T1.FirstName, City = t.T2.City }))
             .Execute();
```

Insert:
```
var address1 = new Address() { AddressId = 1, Line1 = "5555 North Bride Ave", City = "Gary", State = "IN" };
var address2 = new Address() { AddressId = 2, Line1 = "5555 North Bride Rd.", City = "Chicago", State = "IL" };

var rowCount = qb.Insert(q => q.Into(t => t.Addresses, t => new { t.AddressId, t.Line1, t.City, t.State })
                               .Values(address1)
                               .Values(address2))
                 .Execute();
```

Update:
```
var rowCount = qb.Update(q => q.From(t => t.Addresses)
                               .Set(t => t.State, "IN"))
                 .Execute();
```

Delete:
```
var rowCount = qb.Delete(q => q.From(t => t.Users)
                               .Where(t => t.T1.FirstName != "Josh"))
                 .Execute();
```
