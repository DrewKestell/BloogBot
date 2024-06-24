// ActivityManager.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include <iostream>
#include <memory>
#include <array>
#include <asio.hpp>
#include <asio/experimental/as_tuple.hpp>
#include <asio/experimental/awaitable_operators.hpp>
#include "communication.pb.h"  // Include the compiled protocol buffer header

using asio::awaitable;
using asio::buffer;
using asio::co_spawn;
using asio::detached;
using asio::ip::tcp;
using namespace asio::experimental::awaitable_operators;
using namespace std::literals::chrono_literals;

constexpr auto use_nothrow_awaitable = asio::experimental::as_tuple(asio::use_awaitable);

// Timer for timeout and keep-alive messages
awaitable<void> timeout(asio::steady_timer& timer, std::chrono::steady_clock::duration duration) {
    timer.expires_after(duration);
    co_await timer.async_wait(use_nothrow_awaitable);
}

// Function to serialize and send a protobuf message
template<typename T>
awaitable<void> send_message(tcp::socket& socket, const T& msg) {
    std::string data;
    msg.SerializeToString(&data);
    co_await asio::async_write(socket, buffer(data), use_nothrow_awaitable);
}

// Function to receive and deserialize a protobuf message
template<typename T>
    requires std::is_base_of<google::protobuf::MessageLite, T>::value
awaitable<T> receive_message(tcp::socket& socket) {
    std::array<char, 4096> data; // Buffer for incoming data
    T msg;
    auto length = co_await socket.async_read_some(buffer(data), asio::use_awaitable);
    if (!msg.ParseFromArray(data.data(), length)) {
        throw std::runtime_error("Failed to parse message");
    }
    co_return msg;
}

// Handle communication over a socket
awaitable<void> handle_client(tcp::acceptor& acceptor) {
    try {
        auto [e, socket] = co_await acceptor.async_accept(use_nothrow_awaitable);
        asio::steady_timer timer(socket.get_executor());
        while (socket.is_open()) {
            auto msg = co_await receive_message<communication::UniversalMessage>(socket);

            if (msg.has_data()) {
                // Process data message
                std::cout << "Received data message with payload size: " << msg.data().payload().size() << "\n";
            }
            else if (msg.has_control()) {
                // Process control message
                std::cout << "Received control message with error code: " << msg.control().error_code() << "\n";
            }

            // Send a keep-alive message using the ControlMessage
            communication::ControlMessage keep_alive;
            keep_alive.set_error_code(0); // No error, just a keep-alive
            keep_alive.set_error_description("Keep-alive");

            communication::UniversalMessage response;
            response.mutable_control()->CopyFrom(keep_alive);

            co_await send_message(socket, response);

            // Reset timer for 5 seconds
            co_await timeout(timer, 5s);
        }
    }
    catch (std::exception& e) {
        std::cerr << "Error handling client: " << e.what() << "\n";
    }
}

int main(int argc, char* argv[]) {
    try {
        if (argc != 2) {
            std::cerr << "Usage: ActivityManager <port>\n";
            return 1;
        }

        asio::io_context io_context;

        tcp::endpoint endpoint(tcp::v4(), std::atoi(argv[1]));
        tcp::acceptor acceptor(io_context, endpoint);

        while (true) {
            co_spawn(io_context, handle_client(acceptor), detached);
        }

        io_context.run();
    }
    catch (std::exception& e) {
        std::cerr << "Exception: " << e.what() << "\n";
    }
}



// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
