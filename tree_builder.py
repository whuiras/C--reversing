import anytree

def build_trees(super_classes, class_declarations):
    ctrees = []

    # check for top level classes and create root nodes
    for i in range(len(super_classes)):
        is_top_level = True
        for j in range(int(len(class_declarations))):
            class_names = class_declarations[j].split(' : ')

            # if it does inherit, label it as a lower level class
            if len(class_names) > 1:
                if super_classes[i] in class_names[0]:
                    is_top_level = False

        if is_top_level:
            ctrees.append(anytree.Node(str(super_classes[i])))

    fill_trees(ctrees, class_declarations)

    return ctrees


def fill_trees(nodes, class_declarations):
    for node in nodes:
        child = None
        for class_dec in class_declarations:
            class_names = class_dec.split(' : ')

            if len(class_names) > 1 and node.name == class_names[1]:
                child = anytree.Node(str(class_names[0]), parent=node)

        if child is not None:
            fill_trees([child], class_declarations)
