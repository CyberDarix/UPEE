// UPEE_RUN_CPP_PRIORITY=2

#include <iostream>
#include <chrono>
#include <iomanip>

int main() {
	std::cout << std::string(60, '=') << std::endl;
	std::cout << "UPEE C++ Script - System Information" << std::endl;
	std::cout << std::string(60, '=') << std::endl;

	auto now = std::chrono::system_clock::now();
	auto time = std::chrono::system_clock::to_time_t(now);

	std::cout << "\n[INFO] Execution timestamp: " << std::put_time(std::localtime(&time), "%Y-%m-%d %H:%M:%S") << std::endl;
	std::cout << "[INFO] Compiler: " << __cplusplus << std::endl;
	std::cout << "[INFO] Platform: " << __DATE__ << " " << __TIME__ << std::endl;

	std::cout << "\n[STATUS] Hello from UPEE C++ Runtime!" << std::endl;
	std::cout << std::string(60, '=') << std::endl;

	return 0;  // Success
}
