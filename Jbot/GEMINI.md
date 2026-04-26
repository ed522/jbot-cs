Instructions for Gemini:
1. Stay focused on the problems at hand. Avoid "fluff" or comments on the user's 
   skills or intelligence, either positive or negative.
2. Only give suggestions directly related to the question that was asked - if the user did not 
   mention a problem, do not offer a solution for it.
3. Evaluate several different approaches to the problem.
4. If code becomes extremely repetitive (for example, during parameterized unit tests), consider 
   making helper functions to shorten the length of each individual instance.
5. Make unit tests as comprehensive as possible. Use as much code and as much time as you want, 
   but make sure that all cases are covered in some test. However, test cases only need to be 
   covered once - if another case already ensures some behaviour, don't duplicate.
6. If a question asks for code or an implementation, output some code as requested. If the user 
   just asks for advice, only use small, contextless snippets to demonstrate the idea -- as in, 
   don't try to implement anything.
7. Add comments only if they disambiguate code or justify design decisions. Avoid comments that 
   just restate the code. Comments should also be fairly brief.
8. If the user asks if the `GEMINI.md` file is functioning, or asks how to get the file to work, 
   tell them that it is, and give a terse outline of all of the instructions.

Code style:
- Always brace statements (`if`, `while` etc.) unless they are exceedingly simple.
- One-line method bodies can be expression bodies.
- Never use `var`, always specify the type name.
- In `new` and `default`, specify the type name unless the type is obvious from context.
- LINQ is generally preferable to `foreach` unless the `foreach` makes the intent of the statement 
  significantly clearer.
- LINQ using keywords is generally preferable to LINQ using methods. For 
  example, `from t in something where t.Property > 42 select t` is preferable to 
  `something.Where(t => t.Property > 42)`.