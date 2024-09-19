In the attached solution we have an example of a service.
This service finds matches between 2 records – each record can be matched to only one other record.

Two records will be matched if ALL the below conditions are fulfilled, the conditions are:

1.	Symbol fields are identical
2.	Quantity fields are identical or have a tolerance value (up and down)
a.	For example (tolerance here is set to 10):
i.	Quantity1 = 100 & Quantity2 = 100 -> True (Valid for matching)
ii.	Quantity1 = 110 & Quantity2 = 100 -> True (Valid for matching)
iii.	Quantity1 = 100 & Quantity2 = 111 -> False (Not valid for matching)
3.	Price fields should be identical
4.	Date fields should be identical
5.	Type fields should be opposite (Valid values – Buy/Sell)
a.	For example:
i.	Type1 = Buy & type2 = Sell -> True (Valid for matching)
ii.	Type1 = Buy & type2 = Buy -> False (Not valid for matching)

Record example – 
•	Symbol: Google
•	Quantity: 10000
•	Price: 1250$
•	Date: 05/03/23
•	Type: Buy

Please write tests for the above service and try to find and fix all the bugs in the service 
There is a sample test in the solution.

Good Luck!


[CoreAutomation Test.docx](https://github.com/user-attachments/files/16928030/CoreAutomation.Test.docx)


