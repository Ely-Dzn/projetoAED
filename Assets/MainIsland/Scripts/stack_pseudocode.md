
- StackGroup
    - #stacksContainer
        - Stack [0+]
- Stack
    - #itemsContainer

        itens apenas para inicialização
    - #slotsContainer
        - slot [0+]
            - item [?]

```cs
class StackGroup {
    GameObject stacksContainer;
    ArrayList<Stack> stacks;
    ArrayList<GameObject> transit;
    ArrayList<GameObject> transitTo;
    // StackAnimator animator;
    StackGroup() {
        stacks = stacksContainer.getChildren();
    }
    void Move(int from, int to) {
        var stackFrom = stacks[from];
        var stackTo = stacks[to];
        if (stackFrom.IsEmpty()) {
            //TODO: SHOW ERROR
            return;
        }
        GameObject item = stackFrom.Top();
        Push(to, item);
        stackFrom.Pop();
    }
    void Push(int to, GameObject item) {
        if (stackTo.IsFull()) {
            //TODO: SHOW ERROR
            return;
        }
        stackTo.Push(item);
        transit.add(item);
        transitTo.add(stackTo.getSlot(stackTo.size() - 1));
    }
    void Update() {
        for (var item : transit) {
            var from = item.transform;
            var to = transitTo[item].transform;
            from.position = Vector3.Lerp(from.position, to, Time.deltaTime);
            from.rotation = Quaternion.Lerp(from.rotation, to, Time.deltaTime);
            if (Vector3.Distance(from.position, to) < 0.1f) {
                from.position = to;
                transit.remove(item);
            }
        }
    }
}
class Stack {
    GameObject slotsContainer;
    ArrayList<GameObject> slots;
    ArrayList<GameObject> items;
    int maxSize;
    Stack() {
        slots = slotsContainer.getChildren();
    }
    void Push(GameObject item) {
        items.add(item);
        item.SetParent(this);
    }
    void Pop() {
        int i = items.size() - 1;
        GameObject item = items[i];
        items.remove(i);
        return item;
    }
    void Top() {
        int i = items.size() - 1;
        return items[i];
    }
    bool IsEmpty() {
        return items.size() <= 0;
    }
    bool IsFull() {
        return items.size() >= maxSize;
    }
}
// class StackAnimator {
//     void Move(GameObject item, Vector3 to) {
//         transitTo[item] = to;
//     }
// }
// class BookStackAnimator extends StackAnimator {

// }
```