#include "Table.h"

template<typename K, typename V>
V& Table<K, V>::getCreate(const K& key) {
    return _map[key];
}

template<typename K, typename V>
bool Table<K, V>::containsKey(const K& key) const {
    return _map.find(key) != _map.end();
}

template<typename K, typename V>
void Table<K, V>::set(const K& key, const V& value) {
    _map[key] = value;
}

template<typename K, typename V>
bool Table<K, V>::get(const K& key, V& outValue) const {
    auto it = _map.find(key);
    if (it != _map.end()) {
        outValue = it->second;
        return true;
    }
    return false;
}

// Explicit instantiations (required if compiled separately)
template class Table<std::string, unsigned int>; // Replace types as needed
