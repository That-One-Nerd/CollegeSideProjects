## PropositionReducer

This is a quick project I scraped together in about a day.
- This program will parse a proposition expression in TeX format and evaluate its truth tables.
- If you supply two expressions, it will determine if they are equivalent.
- Uses objects to represent unary and binary operators.

Here's an example of the parser in action:
`(p \and q) \or ((\neg p) \and q)`
is recognized by the program as
`(p ∧ q) ∨ ((¬p) ∧ q)`.

Note that this program **always evaluates operators right to left!** So unless you include parenthesis,
it might act weird. In addition, due to a nuance with the unary parser, `\neg` applies to **everything**
after it, until an end parenthesis. `\neg p \and q` will be treated as `\neg (p \and q)`. So make sure
you have your parenthesis!

The name is a misnomer. At the time of writing, this project does NOT reduce a proposition down.
I am not sure if I intend to develop that feature, it doesn't sound particularly fun.
