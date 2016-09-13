import "./multisig.sol";

contract ColorCoin {

    address owner;

    mapping (address => uint) public coinBalance;
    mapping (address => uint) public coinBalanceMultisig;

    event CoinCashIn(address caller, uint amount);
    event CoinCashOut(address caller, address from, uint amount);
    event CoinTransfer(address caller, address from, address to, uint amount);
    event CoinTransferMultisig(address caller, address from, address to, uint amount);

    function ColorCoin(uint amount) {
        owner = msg.sender;
        coinBalanceMultisig[owner] = amount;
    }

    // transfer private coins to any address
    function transferPrivate(address receiver, uint amount) {
        if (coinBalance[msg.sender] < amount) {
            throw;
        }

        coinBalance[msg.sender] -= amount;
        coinBalance[receiver] += amount;

        CoinTransfer(msg.sender, msg.sender, receiver, amount);
    }

    function cashin(uint amount) {
        if (coinBalance[msg.sender] < amount) {
            throw;
        }

        coinBalance[msg.sender] -= amount;
        coinBalanceMultisig[msg.sender] += amount;

        CoinCashIn(msg.sender, amount);
    }

    // cashout coins (called only from exchange contract)
    function cashout(address from, uint amount) {
        if (coinBalanceMultisig[from] < amount) {
            throw;
        }

        coinBalanceMultisig[from] -= amount;
        coinBalance[from] += amount;

        CoinCashOut(msg.sender, from, amount);
    }

    // transfer coins (called only from exchange contract)
    function transferMultisig(address from, address to, uint amount) {
        if (coinBalanceMultisig[from] < amount) {
            throw;
        }
        
        coinBalanceMultisig[from] -= amount;
        coinBalanceMultisig[to] += amount;

        CoinTransferMultisig(msg.sender, from, to, amount);
    }
}