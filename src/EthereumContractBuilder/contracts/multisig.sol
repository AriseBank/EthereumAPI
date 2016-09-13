import "./coinContract.sol";

contract MainExchange {

    struct TransferTransaction {
        uint id;
        address client_a;
        address client_b;
        address coin_a;
        address coin_b;
        uint amount_a;
        uint amount_b;
        
        bool client_a_approved;
        bool client_b_approved;
    }

    address owner;

    function MainExchange() {
        owner = msg.sender;
    }

    event TransferDone(address sender, address from, address to, uint amount);
    event ConfirmationNeed(address client, bytes32 hash);
    event Debug(string msg);

    modifier onlyowner { if (msg.sender == owner) _ }

    // can be called only from contract owner
    function swap(uint id, address client_a, address client_b, address coinAddress_a, 
                         address coinAddress_b, uint amount_a, uint amount_b) onlyowner {

        var operation = sha3(msg.data, block.number);

        _pending[operation].id = id;
        _pending[operation].client_a = client_a;
        _pending[operation].client_b = client_b;
        _pending[operation].coin_a = coinAddress_a;
        _pending[operation].coin_b = coinAddress_b;
        _pending[operation].amount_a = amount_a;
        _pending[operation].amount_b = amount_b;

        ConfirmationNeed(client_a, operation);
        ConfirmationNeed(client_b, operation);
    }

    function confirm(bytes32 operation) {
        var transaction = _pending[operation];
        if (transaction.id == uint(0)) {
            throw;
        }

        Debug("started confirm");
        if (transaction.client_a == msg.sender) {
            if (!transaction.client_a_approved) {
                transaction.client_a_approved = true;
                Debug("confirmed a");
            }
        }

        if (transaction.client_b == msg.sender) {
            if (!transaction.client_b_approved) {
                transaction.client_b_approved = true;
                Debug("confirmed b");
            }
        }

        if (transaction.client_a_approved && transaction.client_b_approved) {
            internalTransfer(operation);
        }
    }

    function internalTransfer(bytes32 operation) private {
        var transaction = _pending[operation];
        Debug("start transfer");

        transferCoin(transaction.coin_a, transaction.client_a, transaction.client_b, transaction.amount_a);

        transferCoin(transaction.coin_b, transaction.client_b, transaction.client_a, transaction.amount_b);

        Debug("end transfer!");
    }

    function transferCoin(address contractAddress, address from, address to, uint amount) private {
        var coin_contract = ColorCoin(contractAddress);
        coin_contract.transferMultisig(from, to, amount);
    }

    mapping (bytes32 => TransferTransaction) _pending;
}