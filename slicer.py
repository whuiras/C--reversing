import anytree
import os
import tree_builder
import re
import os.path
from os import path


def get_classes(lines):
    class_declarations = []
    classes = []
    interfaces = []
    super_classes = []
    for i in range(int(len(lines))):
        if len(lines[i]) >= 12:
            found = lines[i].find("public class", 0, 12)
            if found > -1:
                line = lines[i].rstrip('\n')
                line = line.lstrip('public class ')
                class_declarations.append(line)
                class_names = line.split(' : ')
                classes.append(class_names[0])

                if len(class_names) > 1:
                    inherited = class_names[1].split(', ')
                    for name in inherited:
                        if name.startswith('I'):
                            interfaces.append(name)
                        else:
                            super_classes.append(name)

    super_classes = list(dict.fromkeys(super_classes))
    interfaces = list(dict.fromkeys(interfaces))

    classes.sort()
    super_classes.sort()
    interfaces.sort()
    class_declarations.sort()

    return classes, super_classes, interfaces, class_declarations


def splice_class(lines, class_name):
    to_find = "^public class " + class_name + " :"
    start = -1
    for i in range(len(lines)):
        if re.search(to_find, lines[i]) is not None:
            start = i
            break
    
    if start == -1:
        print("class not found")
        return None, None
    
    end = find_end(lines, start)
    return start, end
   
 
def find_end(lines, start):
    brackets = 1
    end = start + 2
    while brackets != 0:
        line = lines[end]
        brackets += line.count('{')
        brackets -= line.count('}')
        end += 1
    return end
        

def main():
    with open('Assembly-CSharp.decompiled.cs', 'r') as f:
        lines = f.readlines()
        classes, super_classes, interfaces, class_decs = get_classes(lines)
        ctrees = tree_builder.build_trees(super_classes, class_decs)
        print_trees(ctrees)
        
        class_name = "AttackInfo"
        start, end = splice_class(lines, class_name)
        if start is not None:
            pass
            write_class(lines, class_name.lower(), start, end)
        
        # print_classes(classes, super_classes, interfaces)
        
        
def write_class(lines, name, start, end):
    if start == end:
        return
    
    filename = os.path.join("annotated", name + ".cs")
    if path.exists(filename):
        return
    
    with open(filename, 'w') as f:
        for i in range(start, end):
            f.write(lines[i])


def print_classes(classes, super_classes, interfaces):
    print('Classes are: ')
    print()
    for class_name in classes:
        print(class_name)
    print()
    print()
    print('Super classes are: ')
    print()
    for super_class in super_classes:
        print(super_class)
    print()
    print()
    print('Interfaces are: ')
    print()
    for interface in interfaces:
        print(interface)
    print()
    print()


def print_trees(trees):
    for tree in trees:
        print()
        for pre, fill, node in anytree.RenderTree(tree):
            print("%s%s" % (pre, node.name))


if __name__ == '__main__':
    main()
