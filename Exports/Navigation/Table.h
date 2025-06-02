#pragma once

#include <unordered_map>
#include <string>

template<typename K, typename V>
class Table {
public:
    using MapType = std::unordered_map<K, V>;
    using iterator = typename MapType::iterator;
    using const_iterator = typename MapType::const_iterator;

    Table() = default;

    V& getCreate(const K& key);
    bool containsKey(const K& key) const;
    void set(const K& key, const V& value);
    bool get(const K& key, V& outValue) const;

    iterator begin() { return _map.begin(); }
    iterator end() { return _map.end(); }
    const_iterator begin() const { return _map.begin(); }
    const_iterator end() const { return _map.end(); }

private:
    MapType _map;
};
