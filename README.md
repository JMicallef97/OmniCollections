OmniBuffer is a class which wraps around a List<T> object and accesses it as a circular list. Features include:

1) Ability to convert a regular list into a circular list (that is, when requesting items at
   indices larger than the list count, the index will "wrap around" to the start of the list).
2) Ability to traverse a list in reverse (as if the list items were reversed) by changing a single variable.
3) Ability to define an arbitrary starting point (the item at index 0) in the list.
4) Restrict a list to being read-only.