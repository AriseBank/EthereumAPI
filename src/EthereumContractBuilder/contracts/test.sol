pragma solidity ^0.4.1;

contract Test {
    function Test() {

    }

    function getAddressFromSig(bytes32 hash, bytes sig) private returns(address) {
        bytes32 r;
        bytes32 s;
        uint8 v;

        assembly {
            r := mload(add(sig, 32))
            s := mload(add(sig, 64))
            v := mload(add(sig, 65))
        }

        return ecrecover(hash, v, r, s);
    }

    function checkSig(address addr, bytes32 hash, bytes sig) returns(bool) {
        return addr == getAddressFromSig(hash, sig);
    }
}
 