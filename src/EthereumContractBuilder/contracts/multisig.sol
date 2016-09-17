pragma solidity ^0.4.1;
import "./coin.sol";

contract MainExchange {

    struct CashoutTransaction {
        bool exists;
        address from;
        address to; // only for ETH coin contract
        address coin;
        uint amount;
    } 

    function MainExchange() {
        _owner = msg.sender;
    }

    event ConfirmationNeed(address client, bytes32 hash);
    event Debug(string msg);

    modifier onlyowner { if (msg.sender == _owner) _; }

    // can be called only from contract owner
    // create swap transaction signed by exchange
    function swap(address client_a, address client_b, address coinAddress_a, address coinAddress_b, uint amount_a, uint amount_b,
                    bytes32 hash, bytes client_a_sign, bytes client_b_sign) onlyowner returns(bool) {
        
        if (!_checkClientSign(client_a, hash, client_a_sign)) {
            throw;                    
        }
        if (!_checkClientSign(client_b, hash, client_b_sign)) {
            throw;
        }

        _transferCoins(coinAddress_a, client_a, client_b, amount_a, hash, client_a_sign);
        _transferCoins(coinAddress_b, client_b, client_a, amount_b, hash, client_b_sign);

        return true;
    }

    function cashout(uint id, address coinContractAddress, address from, uint amount, address to) onlyowner {
         // calculate swap transaction hash (id always incremented)
        var operation = sha3(msg.data, id);
        throw;
        _pendingCashout[operation].from = from;
        _pendingCashout[operation].coin = coinContractAddress;
        _pendingCashout[operation].amount = amount;
        _pendingCashout[operation].to = to;
        _pendingCashout[operation].exists = true;

        ConfirmationNeed(from, operation);
    }

    // change coin exchange contract
    function changeExchangeContract(address coinContract, address newExchangeContract) onlyowner {
        var coin_contract = Coin(coinContract);
        coin_contract.changeExchangeContract(newExchangeContract);
    }
    
    function _internalCashout(bytes32 operation) private {
        var transaction = _pendingCashout[operation];

        var coin_contract = Coin(transaction.coin);
        coin_contract.cashout(operation, transaction.from, transaction.amount, transaction.to);
    }

    function _transferCoins(address contractAddress, address from, address to, uint amount, bytes32 hash, bytes sig) private {
        var coin_contract = Coin(contractAddress);
        coin_contract.transferMultisig(from, to, amount, hash, sig);
    }

    function _checkClientSign(address client_addr, bytes32 hash, bytes sig) returns(bool) {
        bytes32 r;
        bytes32 s;
        uint8 v;

        assembly {
            r := mload(add(sig, 32))
            s := mload(add(sig, 64))
            v := mload(add(sig, 65))
        }

        return client_addr == ecrecover(hash, v, r, s);
    }

    //private fields

    address _owner;

    mapping (bytes32 => CashoutTransaction) _pendingCashout;
}