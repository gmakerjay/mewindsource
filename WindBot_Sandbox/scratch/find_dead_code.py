import os
import re
import ast

PROJECT_ROOT = "C:\\Users\\admin\\Documents\\EDOTh"
WINDBOT_DIR = os.path.join(PROJECT_ROOT, "WindBot")
SANDBOX_DIR = os.path.join(PROJECT_ROOT, "WindBot_Sandbox")

def audit_cs_file():
    cs_path = os.path.join(WINDBOT_DIR, "UnifiedIgnisExecutor.cs")
    if not os.path.exists(cs_path):
        print("C# file not found.")
        return []
        
    with open(cs_path, "r", encoding="utf-8") as f:
        content = f.read()

    # Find private method declarations
    # Match: private <type> <name>(...)
    # We also want to exclude common ones or ones that might be callbacks, but private methods must be called inside the class.
    method_decl_pattern = re.compile(r'private\s+[\w\<\>\[\]\?`]+\s+(\w+)\s*\(')
    private_methods = set(method_decl_pattern.findall(content))

    # Find private field declarations
    # Match: private <type> <name>; or private <type> <name> = ...;
    field_decl_pattern = re.compile(r'private\s+(?:static\s+)?(?:readonly\s+)?[\w\<\>\[\]\?`]+\s+(\w+)\s*(?:;|=)')
    private_fields = set(field_decl_pattern.findall(content))

    # Filter out keywords/false positives
    false_positives = {"void", "bool", "int", "string", "double", "float", "long", "object"}
    private_methods = {m for m in private_methods if m not in false_positives}
    private_fields = {f for f in private_fields if f not in false_positives}

    unused_methods = []
    for method in private_methods:
        # Find how many times this method name appears in the file
        # It must appear at least twice: once for declaration, and at least once for reference.
        # If it appears only once, it is unused.
        occurrences = len(re.findall(r'\b' + re.escape(method) + r'\b', content))
        if occurrences <= 1:
            unused_methods.append(method)

    unused_fields = []
    for field in private_fields:
        occurrences = len(re.findall(r'\b' + re.escape(field) + r'\b', content))
        if occurrences <= 1:
            unused_fields.append(field)

    return unused_methods, unused_fields


def audit_py_files():
    unused_imports = {}
    unused_functions = {}
    
    for entry in os.listdir(SANDBOX_DIR):
        if not entry.endswith(".py"):
            continue
        py_path = os.path.join(SANDBOX_DIR, entry)
        
        with open(py_path, "r", encoding="utf-8") as f:
            code = f.read()
            
        try:
            tree = ast.parse(code, filename=entry)
        except Exception as e:
            print(f"Error parsing {entry}: {e}")
            continue

        # Track defined functions, classes and imports
        imports = []
        functions = []
        
        class Analyzer(ast.NodeVisitor):
            def visit_Import(self, node):
                for alias in node.names:
                    imports.append((alias.asname or alias.name, node.lineno))
                self.generic_visit(node)
                
            def visit_ImportFrom(self, node):
                for alias in node.names:
                    imports.append((alias.asname or alias.name, node.lineno))
                self.generic_visit(node)
                
            def visit_FunctionDef(self, node):
                # Don't flag main or double underscore methods
                if node.name != "main" and not node.name.startswith("__"):
                    functions.append((node.name, node.lineno))
                self.generic_visit(node)

        analyzer = Analyzer()
        analyzer.visit(tree)

        # Count references in the AST
        # To find references, we can check all Name and Attribute nodes
        referenced_names = set()
        class ReferenceFinder(ast.NodeVisitor):
            def visit_Name(self, node):
                if isinstance(node.ctx, ast.Load):
                    referenced_names.add(node.id)
                self.generic_visit(node)
                
            def visit_Attribute(self, node):
                # E.g. object.method -> check if 'method' is reference
                referenced_names.add(node.attr)
                self.generic_visit(node)
                
        ReferenceFinder().visit(tree)

        # Check imports
        file_unused_imports = []
        for imp, line in imports:
            if imp not in referenced_names:
                file_unused_imports.append((imp, line))
        if file_unused_imports:
            unused_imports[entry] = file_unused_imports

        # Check functions
        file_unused_funcs = []
        for func, line in functions:
            # Check if function name is referenced anywhere else in the file
            # Since AST visitor checks references, if func is in referenced_names, it means it is referenced.
            # But wait, the function definition itself doesn't count as a load.
            if func not in referenced_names:
                file_unused_funcs.append((func, line))
        if file_unused_funcs:
            unused_functions[entry] = file_unused_funcs

    return unused_imports, unused_functions

if __name__ == "__main__":
    print("--- C# AUDIT ---")
    cs_methods, cs_fields = audit_cs_file()
    print(f"Unused private methods ({len(cs_methods)}):")
    for m in cs_methods:
        print(f"  - {m}")
    print(f"Unused private fields ({len(cs_fields)}):")
    for f in cs_fields:
        print(f"  - {f}")

    print("\n--- PYTHON AUDIT ---")
    py_imports, py_funcs = audit_py_files()
    print("Unused imports:")
    for file, imps in py_imports.items():
        print(f"  In {file}:")
        for imp, line in imps:
            print(f"    - {imp} (line {line})")
            
    print("Unused functions:")
    for file, funcs in py_funcs.items():
        print(f"  In {file}:")
        for func, line in funcs:
            print(f"    - {func} (line {line})")
